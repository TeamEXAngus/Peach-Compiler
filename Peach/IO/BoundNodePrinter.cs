using Peach.CodeAnalysis.Symbols;
using Peach.CodeAnalysis.Syntax;
using Peach.IO;
using System.CodeDom.Compiler;
using System.IO;

namespace Peach.CodeAnalysis.Binding
{
    internal static class BoundNodePrinter
    {
        public static void WriteTo(this BoundNode node, TextWriter writer)
        {
            if (writer is IndentedTextWriter iw)
                WriteTo(node, iw);
            else
                WriteTo(node, new IndentedTextWriter(writer));
        }

        public static void WriteTo(this BoundNode node, IndentedTextWriter writer)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    WriteBlockStatement(node as BoundBlockStatement, writer);
                    break;

                case BoundNodeKind.ExpressionStatement:
                    WriteExpressionStatement(node as BoundExpressionStatement, writer);
                    break;

                case BoundNodeKind.VariableDeclaration:
                    WriteVariableDeclaration(node as BoundVariableDeclaration, writer);
                    break;

                case BoundNodeKind.IfStatement:
                    WriteIfStatement(node as BoundIfStatement, writer);
                    break;

                case BoundNodeKind.WhileStatement:
                    WriteWhileStatement(node as BoundWhileStatement, writer);
                    break;

                case BoundNodeKind.LoopStatement:
                    WriteLoopStatement(node as BoundLoopStatement, writer);
                    break;

                case BoundNodeKind.ForStatement:
                    WriteForStatement(node as BoundForStatement, writer);
                    break;

                case BoundNodeKind.GotoStatement:
                    WriteGotoStatement(node as BoundGotoStatement, writer);
                    break;

                case BoundNodeKind.ConditionalGotoStatement:
                    WriteConditionalGotoStatement(node as BoundConditionalGotoStatement, writer);
                    break;

                case BoundNodeKind.LabelStatement:
                    WriteLabelStatement(node as BoundLabelStatement, writer);
                    break;

                case BoundNodeKind.BinaryExpression:
                    WriteBinaryExpression(node as BoundBinaryExpression, writer);
                    break;

                case BoundNodeKind.LiteralExpression:
                    WriteLiteralExpression(node as BoundLiteralExpression, writer);
                    break;

                case BoundNodeKind.ParenthesisedExpression:
                    WriteParenthesisedExpression(node as BoundParenthesisedExpression, writer);
                    break;

                case BoundNodeKind.VariableExpression:
                    WriteVariableExpression(node as BoundVariableExpression, writer);
                    break;

                case BoundNodeKind.AssignmentExpression:
                    WriteAssignmentExpression(node as BoundAssignmentExpression, writer);
                    break;

                case BoundNodeKind.UnaryExpression:
                    WriteUnaryExpression(node as BoundUnaryExpression, writer);
                    break;

                case BoundNodeKind.FunctionCallExpression:
                    WriteFunctionCallExpression(node as BoundFunctionCallExpression, writer);
                    break;

                case BoundNodeKind.TypeCastExpression:
                    WriteTypeCastExpression(node as BoundTypeCastExpression, writer);
                    break;

                case BoundNodeKind.ReturnStatement:
                    WriteReturnExpression(node as BoundReturnStatement, writer);
                    break;

                case BoundNodeKind.ErrorExpression:
                    WriteErrorExpression(node as BoundErrorExpression, writer);
                    break;

                default:
                    throw new System.Exception($"Unexpected node kind {node.Kind}");
            }
        }

        private static void WriteNestedStatement(this IndentedTextWriter writer, BoundNode node)
        {
            var needsIndentation = !(node is BoundBlockStatement);

            if (needsIndentation)
                writer.Indent += 2;

            node.WriteTo(writer);

            if (needsIndentation)
                writer.Indent -= 2;
        }

        private static void WriteBlockStatement(BoundBlockStatement node, IndentedTextWriter writer)
        {
            writer.WritePunctuation(SyntaxKind.OpenBraceToken);
            writer.WriteLine();
            writer.Indent += 2;

            foreach (var s in node.Statements)
                s.WriteTo(writer);

            writer.Indent -= 2;
            writer.WritePunctuation(SyntaxKind.CloseBraceToken);
            writer.WriteLine();
        }

        private static void WriteExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer)
        {
            node.Expression.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteVariableDeclaration(BoundVariableDeclaration node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(node.Variable.IsConst ? SyntaxKind.ConstKeyword : SyntaxKind.LetKeyword);
            writer.WriteSpace();
            writer.WriteIdentifier(node.Variable.Name);
            writer.WritePunctuation(SyntaxKind.ColonToken);
            writer.WriteSpace();
            writer.WriteKeyword(node.Variable.Type.ToString());
            writer.WriteSpace();
            writer.WritePunctuation(SyntaxKind.EqualsToken);
            writer.WriteSpace();
            node.Initializer.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteIfStatement(BoundIfStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(SyntaxKind.IfKeyword);
            writer.WriteSpace();
            if (node.IsNegated)
            {
                writer.WriteKeyword(SyntaxKind.NotKeyword);
                writer.WriteSpace();
            }
            node.Condition.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.ThenStatment);
            if (node.ElseStatement is not null)
            {
                writer.WriteKeyword(SyntaxKind.ElseKeyword);
                writer.WriteLine();
                writer.WriteNestedStatement(node.ElseStatement);
            }
            writer.WriteLine();
        }

        private static void WriteWhileStatement(BoundWhileStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(SyntaxKind.WhileKeyword);
            writer.WriteSpace();
            if (node.IsNegated)
            {
                writer.WriteKeyword(SyntaxKind.NotKeyword);
                writer.WriteSpace();
            }
            node.Condition.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
            writer.WriteLine();
        }

        private static void WriteLoopStatement(BoundLoopStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(SyntaxKind.LoopKeyword);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
            writer.WriteLine();
        }

        private static void WriteForStatement(BoundForStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(SyntaxKind.ForKeyword);
            writer.WriteSpace();
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WriteKeyword(SyntaxKind.ForKeyword);
            writer.WriteSpace();
            node.Start.WriteTo(writer);
            writer.WriteSpace();
            writer.WriteKeyword(SyntaxKind.ToKeyword);
            writer.WriteSpace();
            node.Stop.WriteTo(writer);
            writer.WriteSpace();
            writer.WriteKeyword(SyntaxKind.StepKeyword);
            writer.WriteSpace();
            node.Step.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
        }

        private static void WriteGotoStatement(BoundGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteGotoLabel("goto");
            writer.WriteSpace();
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteLine();
        }

        private static void WriteConditionalGotoStatement(BoundConditionalGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteGotoLabel("goto");
            writer.WriteSpace();
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteSpace();
            writer.WriteKeyword(SyntaxKind.IfKeyword);
            writer.WriteSpace();
            if (!node.JumpIfTrue)
            {
                writer.WriteKeyword(SyntaxKind.NotKeyword);
                writer.WriteSpace();
            }
            node.Condition.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteLabelStatement(BoundLabelStatement node, IndentedTextWriter writer)
        {
            writer.Indent -= 2;
            writer.WriteGotoLabel(node.Label.Name);
            writer.WritePunctuation(SyntaxKind.ColonToken);
            writer.WriteSpace();
            writer.Indent += 2;
            writer.WriteLine();
        }

        private static void WriteBinaryExpression(BoundBinaryExpression node, IndentedTextWriter writer)
        {
            node.Left.WriteTo(writer);
            writer.WriteSpace();
            writer.WritePunctuation(node.Op.SyntaxKind);
            writer.WriteSpace();
            node.Right.WriteTo(writer);
        }

        private static void WriteLiteralExpression(BoundLiteralExpression node, IndentedTextWriter writer)
        {
            if (node.Type == TypeSymbol.Bool)
            {
                writer.WriteKeyword(node.Value.ToString());
            }
            else if (node.Type == TypeSymbol.Int)
            {
                writer.WriteNumber(node.Value.ToString());
            }
            else if (node.Type == TypeSymbol.String)
            {
                const string QuoteMark = "\"";
                const string EscapedQuoteMark = "\\\"";
                writer.WriteString(QuoteMark);
                writer.WriteString(node.Value.ToString().Replace(QuoteMark, EscapedQuoteMark));
                writer.WriteString(QuoteMark);
            }
            else
            {
                writer.WriteKeyword(node.Type.Name);
            }
        }

        private static void WriteParenthesisedExpression(BoundParenthesisedExpression node, IndentedTextWriter writer)
        {
            writer.WritePunctuation(SyntaxKind.OpenParenToken);
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(SyntaxKind.CloseParenToken);
        }

        private static void WriteVariableExpression(BoundVariableExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
        }

        private static void WriteAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteSpace();
            writer.WritePunctuation(SyntaxKind.EqualsToken);
            writer.WriteSpace();
            node.Expression.WriteTo(writer);
        }

        private static void WriteUnaryExpression(BoundUnaryExpression node, IndentedTextWriter writer)
        {
            writer.WritePunctuation(node.Op.SyntaxKind);
            node.Operand.WriteTo(writer);
        }

        private static void WriteFunctionCallExpression(BoundFunctionCallExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Function.Name);
            writer.WritePunctuation(SyntaxKind.OpenParenToken);

            var isFirst = true;
            foreach (var arg in node.Arguments)
            {
                if (isFirst)
                {
                    isFirst = false;
                }
                else
                {
                    writer.WritePunctuation(SyntaxKind.CommaToken);
                    writer.WriteSpace();
                }

                arg.WriteTo(writer);
            }
            writer.WritePunctuation(SyntaxKind.CloseParenToken);
        }

        private static void WriteTypeCastExpression(BoundTypeCastExpression node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(node.Type.Name);
            writer.WritePunctuation(SyntaxKind.OpenParenToken);
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(SyntaxKind.CloseParenToken);
        }

        private static void WriteReturnExpression(BoundReturnStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(SyntaxKind.ReturnKeyword);
            if (node.Expression is not null)
            {
                writer.WriteSpace();
                node.Expression.WriteTo(writer);
            }
            writer.WriteLine();
        }

        private static void WriteErrorExpression(BoundErrorExpression _, IndentedTextWriter writer)
        {
            writer.WriteKeyword("ERROR");
        }
    }
}