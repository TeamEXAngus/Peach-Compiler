using Peach.CodeAnalysis.Binding;
using Peach.CodeAnalysis.Symbols;
using Peach.CodeAnalysis.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Peach.CodeAnalysis.Lowering
{
    internal class Lowerer : BoundTreeRewriter
    {
        private int _labelCount;

        private Lowerer()
        { }

        private BoundLabel GenerateLabel()
        {
            var name = $"Label{++_labelCount}";
            return new BoundLabel(name);
        }

        public static BoundBlockStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            var result = lowerer.RewriteStatement(statement);
            return Flatten(result);
        }

        private static BoundBlockStatement Flatten(BoundStatement statement)
        {
            var builder = ImmutableArray.CreateBuilder<BoundStatement>();
            var stack = new Stack<BoundStatement>();
            stack.Push(statement);

            while (stack.Count > 0)
            {
                var current = stack.Pop();

                if (current is BoundBlockStatement block)
                {
                    foreach (var s in block.Statements.Reverse())
                        stack.Push(s);
                }
                else
                {
                    builder.Add(current);
                }
            }

            return new BoundBlockStatement(builder.ToImmutable());
        }

        protected override BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            if (node.ElseStatement is null)
                return RewriteSimpleIfStatement(node);

            return RewriteIfElseStatement(node);
        }

        private BoundStatement RewriteSimpleIfStatement(BoundIfStatement node)
        {
            /*
                if <condition>      -->         GotoIfFalse <condition> END
                    <then>                      <then>
                                                END:

            */
            var endLabel = GenerateLabel();
            var endLabelStatement = new BoundLabelStatement(endLabel);
            var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, jumpIfTrue: node.IsNegated);

            var result = new BoundBlockStatement(ImmutableArray.Create(gotoFalse, node.ThenStatment, endLabelStatement));
            return RewriteStatement(result);
        }

        private BoundStatement RewriteIfElseStatement(BoundIfStatement node)
        {
            /*
                                                GotoIfFalse <condition> ELSE
                if <condition>      -->         <then>
                    <then>                      Goto END
                else                            ELSE:
                    <elseThen>                  <elseThen>
                                                END:
            */

            var elseLabel = GenerateLabel();
            var elseLabelStatement = new BoundLabelStatement(elseLabel);

            var endLabel = GenerateLabel();
            var endLabelStatement = new BoundLabelStatement(endLabel);

            var gotoFalse = new BoundConditionalGotoStatement(elseLabel, node.Condition, jumpIfTrue: node.IsNegated);
            var gotoEnd = new BoundGotoStatement(endLabel);

            var result = new BoundBlockStatement(ImmutableArray.Create(
                gotoFalse,
                node.ThenStatment,
                gotoEnd,
                elseLabelStatement,
                node.ElseStatement,
                endLabelStatement));
            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            /*
                                                    START:
                while <condition>                   GotoIfFalse <condition> END
                    <body>                          <body>
                                                    Goto START
                                                    END:
            */

            var startLabel = GenerateLabel();
            var startLabelStatement = new BoundLabelStatement(startLabel);

            var endLabel = GenerateLabel();
            var endLabelStatement = new BoundLabelStatement(endLabel);

            var gotoFalse = new BoundConditionalGotoStatement(endLabel, node.Condition, jumpIfTrue: node.IsNegated);
            var gotoStart = new BoundGotoStatement(startLabel);

            var result = new BoundBlockStatement(ImmutableArray.Create(
                startLabelStatement,
                gotoFalse,
                node.Body,
                gotoStart,
                endLabelStatement
                ));

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            /*
                                                                             let <var> = <start>
                for <var> from <start> to <stop> step <step>     -->         let <end> = <stop>      <-- stop should only be calculated once
                {                                                            while <var> < <end>
                    <body>                                                   {
                }                                                                <body>
                                                                                 <var> = <var> + <step>
                                                                             }
            */

            var variableDeclaration = new BoundVariableDeclaration(node.Variable, node.Start);

            var endSymbol = new VariableSymbol("<end>", true, TypeSymbol.Int);
            var endDeclaration = new BoundVariableDeclaration(endSymbol, node.Stop);

            var variableExpression = new BoundVariableExpression(node.Variable);

            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.LessThanToken, TypeSymbol.Int, TypeSymbol.Int),
                new BoundVariableExpression(endSymbol)
            );

            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, TypeSymbol.Int, TypeSymbol.Int),
                        node.Step
                    )
                )
            );

            var whileBody = new BoundBlockStatement(ImmutableArray.Create(node.Body, increment));
            var whileStatement = new BoundWhileStatement(condition, false, whileBody);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(variableDeclaration, endDeclaration, whileStatement));

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteLoopStatement(BoundLoopStatement node)
        {
            /*
                loop                    while true
                {                       {
                    <body>      -->         <body>
                }                       {
            */

            var literal = new BoundLiteralExpresion(true);

            var whileStatement = new BoundWhileStatement(literal, false, node.Body);

            return RewriteStatement(whileStatement);
        }
    }
}