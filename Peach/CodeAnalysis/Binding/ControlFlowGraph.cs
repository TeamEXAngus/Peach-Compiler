using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class ControlFlowGraph
    {
        private ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockBranch> branches)
        {
            Start = start;
            End = end;
            Blocks = blocks;
            Branches = branches;
        }

        public BasicBlock Start { get; }
        public BasicBlock End { get; }
        public List<BasicBlock> Blocks { get; }
        public List<BasicBlockBranch> Branches { get; }

        internal sealed class BasicBlock
        {
            public BasicBlock()
            {
            }

            public BasicBlock(bool isStart)
            {
                IsStart = isStart;
                IsEnd = !isStart;
            }

            public bool IsStart { get; }
            public bool IsEnd { get; }
            public List<BoundStatement> Statements { get; } = new();
            public List<BasicBlockBranch> Incoming { get; } = new();
            public List<BasicBlockBranch> Outgoing { get; } = new();

            public override string ToString()
            {
                if (IsStart)
                    return "<start>";

                if (IsEnd)
                    return "<end>";

                using var writer = new StringWriter();

                foreach (var statement in Statements)
                {
                    statement.WriteTo(writer);
                }

                return writer.ToString();
            }
        }

        internal sealed class BasicBlockBranch
        {
            public BasicBlockBranch(BasicBlock from, BasicBlock to, BoundExpression condition)
            {
                From = from;
                To = to;
                Condition = condition;
            }

            public BasicBlock From { get; }
            public BasicBlock To { get; }
            public BoundExpression Condition { get; }

            public override string ToString()
            {
                if (Condition is null)
                    return "";

                return Condition.ToString();
            }
        }

        internal sealed class BasicBlockBuilder
        {
            private readonly List<BasicBlock> _blocks = new();
            private readonly List<BoundStatement> _statements = new();

            public List<BasicBlock> Build(BoundBlockStatement block)
            {
                foreach (var statement in block.Statements)
                {
                    switch (statement.Kind)
                    {
                        case BoundNodeKind.ExpressionStatement:
                        case BoundNodeKind.VariableDeclaration:
                            _statements.Add(statement);
                            break;

                        case BoundNodeKind.GotoStatement:
                        case BoundNodeKind.ReturnStatement:
                        case BoundNodeKind.ConditionalGotoStatement:
                            _statements.Add(statement);
                            StartBlock();
                            break;

                        case BoundNodeKind.LabelStatement:
                            StartBlock();
                            _statements.Add(statement);
                            break;

                        default:
                            throw new System.Exception($"Invalid statement {statement.Kind}");
                    }
                }

                EndBlock();

                return _blocks.ToList();
            }

            private void StartBlock()
            {
                EndBlock();
            }

            private void EndBlock()
            {
                if (_statements.Any())
                {
                    var block = new BasicBlock();
                    block.Statements.AddRange(_statements);
                    _blocks.Add(block);
                    _statements.Clear();
                }
            }

            public BasicBlockBuilder(/*BoundBlockStatement block*/)
            {
            }
        }

        internal sealed class GraphBuilder
        {
            private readonly Dictionary<BoundStatement, BasicBlock> _blockFromStatement = new();
            private readonly Dictionary<BoundLabel, BasicBlock> _blockFromLabel = new();
            private readonly List<BasicBlockBranch> _branches = new();
            private readonly BasicBlock _start = new(isStart: true);
            private readonly BasicBlock _end = new(isStart: false);

            public ControlFlowGraph Build(List<BasicBlock> blocks)
            {
                if (!blocks.Any())
                    Connect(_start, _end);
                else
                    Connect(_start, blocks.First());

                foreach (var block in blocks)
                {
                    foreach (var statement in block.Statements)
                    {
                        _blockFromStatement.Add(statement, block);
                        if (statement is BoundLabelStatement label)
                        {
                            _blockFromLabel.Add(label.Label, block);
                        }
                    }
                }

                for (int i = 0; i < blocks.Count; i++)
                {
                    var block = blocks[i];
                    var next = i == blocks.Count - 1 ? _end : blocks[i + 1];

                    foreach (var statement in block.Statements)
                    {
                        var isLast = statement == block.Statements.Last();
                        Walk(statement, block, next, isLast);
                    }
                }

            ScanAgain:
                for (int i = 0; i < blocks.Count; i++)
                {
                    BasicBlock block = blocks[i];
                    if (!block.Incoming.Any())
                    {
                        RemoveBlock(blocks, block);
                        goto ScanAgain;
                    }
                }

                blocks.Insert(0, _start);
                blocks.Add(_end);

                return new ControlFlowGraph(_start, _end, blocks, _branches);
            }

            private void RemoveBlock(List<BasicBlock> blocks, BasicBlock block)
            {
                blocks.Remove(block);

                foreach (var branch in block.Incoming)
                {
                    branch.From.Outgoing.Remove(branch);
                    _branches.Remove(branch);
                }

                foreach (var branch in block.Outgoing)
                {
                    branch.To.Incoming.Remove(branch);
                    _branches.Remove(branch);
                }
            }

            private void Walk(BoundStatement statement, BasicBlock current, BasicBlock next, bool isLast)
            {
                switch (statement.Kind)
                {
                    case BoundNodeKind.LabelStatement:
                    case BoundNodeKind.ExpressionStatement:
                    case BoundNodeKind.VariableDeclaration:
                        if (isLast)
                            Connect(current, next);
                        break;

                    case BoundNodeKind.ReturnStatement:
                        Connect(current, _end);
                        break;

                    case BoundNodeKind.GotoStatement:
                        {
                            var _goto = statement as BoundGotoStatement;
                            var toBlock = _blockFromLabel[_goto.Label];
                            Connect(current, toBlock);
                            break;
                        }

                    case BoundNodeKind.ConditionalGotoStatement:
                        {
                            var _goto = statement as BoundConditionalGotoStatement;
                            var thenBlock = _blockFromLabel[_goto.Label];

                            var negatedCondition = Negate(_goto.Condition);
                            var thenCondition = _goto.JumpIfTrue ? _goto.Condition : negatedCondition;
                            var elseCondition = _goto.JumpIfTrue ? negatedCondition : _goto.Condition;

                            Connect(current, thenBlock, thenCondition);
                            Connect(current, next, elseCondition);
                            break;
                        }

                    default:
                        throw new System.Exception($"Invalid statement {statement.Kind}");
                }
            }

            private static BoundExpression Negate(BoundExpression condition)
            {
                if (condition is BoundLiteralExpression l)
                {
                    var value = (bool)l.Value;
                    return new BoundLiteralExpression(!value);
                }

                var notOperator = BoundUnaryOperator.Bind(Syntax.SyntaxKind.ExclamationToken, Symbols.TypeSymbol.Bool);
                var parenthesisedCondition = condition is BoundParenthesisedExpression p
                                           ? p
                                           : new BoundParenthesisedExpression(condition);

                return new BoundUnaryExpression(notOperator, parenthesisedCondition);
            }

            private void Connect(BasicBlock from, BasicBlock to, BoundExpression condition = null)
            {
                if (condition is BoundLiteralExpression l)
                {
                    if ((bool)l.Value)
                        condition = null;
                    else
                        return;
                }

                var branch = new BasicBlockBranch(from, to, condition);
                from.Outgoing.Add(branch);
                to.Incoming.Add(branch);
                _branches.Add(branch);
            }
        }

        public override string ToString()
        {
            using var builder = new StringWriter();
            WriteTo(builder);
            return builder.ToString();
        }

        public void WriteTo(TextWriter writer)
        {
            static string Quote(string text)
            {
                return "\"" + text.TrimEnd().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace(System.Environment.NewLine, "\\l") + "\"";
            }

            writer.WriteLine("digraph G {");

            var blockIds = new Dictionary<BasicBlock, string>();
            for (int i = 0; i < Blocks.Count; i++)
            {
                var id = $"N{i}";
                blockIds.Add(Blocks[i], id);
            }

            foreach (var block in Blocks)
            {
                var id = blockIds[block];
                var label = Quote(block.ToString());
                writer.WriteLine($"    {id} [label = {label} shape = box]");
            }

            foreach (var branch in Branches)
            {
                var fromId = blockIds[branch.From];
                var toId = blockIds[branch.To];
                var label = Quote(branch.ToString());
                writer.WriteLine($"    {fromId} -> {toId} [label = {label}]");
            }

            writer.WriteLine("}");
        }

        public static ControlFlowGraph Create(BoundBlockStatement body)
        {
            var builder = new BasicBlockBuilder();
            var blocks = builder.Build(body);

            var graphBuilder = new GraphBuilder();

            return graphBuilder.Build(blocks);
        }

        public static bool AllPathsReturn(BoundBlockStatement body)
        {
            var graph = Create(body);

            foreach (var branch in graph.End.Incoming)
            {
                var last = branch.From.Statements.Last();
                if (last.Kind != BoundNodeKind.ReturnStatement)
                    return false;
            }

            return true;
        }
    }
}