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
                    return;

                case BoundNodeKind.ExpressionStatement:
                    WriteExpressionStatement(node as BoundExpressionStatement, writer);
                    return;

                case BoundNodeKind.VariableDeclaration:
                    WriteVariableDeclaration(node as BoundVariableDeclaration, writer);
                    return;

                case BoundNodeKind.IfStatement:
                    WriteIfStatement(node as BoundIfStatement, writer);
                    return;

                case BoundNodeKind.WhileStatement:
                    WriteWhileStatement(node as BoundWhileStatement, writer);
                    return;

                case BoundNodeKind.LoopStatement:
                    WriteLoopStatement(node as BoundLoopStatement, writer);
                    return;

                case BoundNodeKind.ForStatement:
                    WriteForStatement(node as BoundForStatement, writer);
                    return;

                case BoundNodeKind.GotoStatement:
                    WriteGotoStatement(node as BoundGotoStatement, writer);
                    return;

                case BoundNodeKind.ConditionalGotoStatement:
                    WriteConditionalGotoStatement(node as BoundConditionalGotoStatement, writer);
                    return;

                case BoundNodeKind.LabelStatement:
                    WriteLabelStatement(node as BoundLabelStatement, writer);
                    return;

                case BoundNodeKind.BinaryExpression:
                    WriteBinaryExpression(node as BoundBinaryExpression, writer);
                    return;

                case BoundNodeKind.LiteralExpression:
                    WriteLiteralExpression(node as BoundLiteralExpression, writer);
                    return;

                case BoundNodeKind.ParenthesisedExpression:
                    WriteParenthesisedExpression(node as BoundParenthesisedExpression, writer);
                    return;

                case BoundNodeKind.VariableExpression:
                    WriteVariableExpression(node as BoundVariableExpression, writer);
                    return;

                case BoundNodeKind.AssignmentExpression:
                    WriteAssignmentExpression(node as BoundAssignmentExpression, writer);
                    return;

                case BoundNodeKind.UnaryExpression:
                    WriteUnaryExpression(node as BoundUnaryExpression, writer);
                    return;

                case BoundNodeKind.FunctionCallExpression:
                    WriteFunctionCallExpression(node as BoundFunctionCallExpression, writer);
                    return;

                case BoundNodeKind.TypeCastExpression:
                    WriteTypeCastExpression(node as BoundTypeCastExpression, writer);
                    return;

                case BoundNodeKind.IndexingExpression:
                    WriteIndexingExpression(node as BoundIndexingExpression, writer);
                    return;

                case BoundNodeKind.ListExpression:
                    WriteListExpression(node as BoundListExpression, writer);
                    return;

                case BoundNodeKind.ErrorExpression:
                    WriteErrorExpression(node as BoundErrorExpression, writer);
                    return;

                default:
                    throw new System.Exception($"Unexpected node kind {node.Kind}");
            }

            writer.WriteLine();
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
            writer.WritePunctuation("{");
            writer.WriteLine();
            writer.Indent += 2;

            foreach (var s in node.Statements)
                s.WriteTo(writer);

            writer.Indent -= 2;
            writer.WritePunctuation("}");
            writer.WriteLine();
        }

        private static void WriteExpressionStatement(BoundExpressionStatement node, IndentedTextWriter writer)
        {
            node.Expression.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteVariableDeclaration(BoundVariableDeclaration node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(node.Variable.IsConst ? "const " : "let ");
            writer.WriteIdentifier(node.Variable.Name);
            writer.WritePunctuation(": ");
            writer.WriteKeyword(node.Variable.Type.ToString());
            writer.WritePunctuation(" = ");
            node.Initializer.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteIfStatement(BoundIfStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(node.IsNegated ? "if not  " : "if ");
            node.Condition.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.ThenStatment);
            if (node.ElseStatement is not null)
            {
                writer.WriteKeyword("else");
                writer.WriteLine();
                writer.WriteNestedStatement(node.ElseStatement);
            }
            writer.WriteLine();
        }

        private static void WriteWhileStatement(BoundWhileStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(node.IsNegated ? "while not  " : "while ");
            node.Condition.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
            writer.WriteLine();
        }

        private static void WriteLoopStatement(BoundLoopStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("loop");
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
            writer.WriteLine();
        }

        private static void WriteForStatement(BoundForStatement node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("for ");
            writer.WriteIdentifier(node.Variable.Name);
            writer.WriteKeyword(" from ");
            node.Start.WriteTo(writer);
            writer.WriteKeyword(" to ");
            node.Stop.WriteTo(writer);
            writer.WriteKeyword(" step ");
            node.Step.WriteTo(writer);
            writer.WriteLine();
            writer.WriteNestedStatement(node.Body);
        }

        private static void WriteGotoStatement(BoundGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteGotoLabel("goto ");
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteLine();
        }

        private static void WriteConditionalGotoStatement(BoundConditionalGotoStatement node, IndentedTextWriter writer)
        {
            writer.WriteGotoLabel("goto ");
            writer.WriteIdentifier(node.Label.Name);
            writer.WriteKeyword(node.JumpIfTrue ? " if " : " if not ");
            node.Condition.WriteTo(writer);
            writer.WriteLine();
        }

        private static void WriteLabelStatement(BoundLabelStatement node, IndentedTextWriter writer)
        {
            writer.Indent--;
            writer.WriteGotoLabel("Label");
            writer.WritePunctuation(": ");
            writer.WriteIdentifier(node.Label.Name);
            writer.Indent++;
            writer.WriteLine();
        }

        private static void WriteBinaryExpression(BoundBinaryExpression node, IndentedTextWriter writer)
        {
            var op = SyntaxFacts.GetText(node.Op.SyntaxKind);
            node.Left.WriteTo(writer);
            writer.WritePunctuation($" {op} ");
            node.Right.WriteTo(writer);
        }

        private static void WriteLiteralExpression(BoundLiteralExpression node, IndentedTextWriter writer)
        {
            if (node.Type == TypeSymbol.Bool)
                writer.WriteKeyword(node.Value.ToString());
            else if (node.Type == TypeSymbol.Int)
                writer.WriteNumber(node.Value.ToString());
            else if (node.Type == TypeSymbol.String)
                writer.WriteString($"{'"'}{node.Value.ToString().Replace("\"", "\\\"")}{'"'}");
            else
                writer.WriteKeyword(node.Type.Name);
        }

        private static void WriteParenthesisedExpression(BoundParenthesisedExpression node, IndentedTextWriter writer)
        {
            writer.WritePunctuation("(");
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(")");
        }

        private static void WriteVariableExpression(BoundVariableExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
        }

        private static void WriteAssignmentExpression(BoundAssignmentExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Variable.Name);
            writer.WritePunctuation(" = ");
            node.Expression.WriteTo(writer);
        }

        private static void WriteUnaryExpression(BoundUnaryExpression node, IndentedTextWriter writer)
        {
            var op = SyntaxFacts.GetText(node.Op.SyntaxKind);
            writer.WritePunctuation(op);
            node.Operand.WriteTo(writer);
        }

        private static void WriteFunctionCallExpression(BoundFunctionCallExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.Function.Name);
            writer.WritePunctuation("(");

            var isFirst = true;
            foreach (var arg in node.Arguments)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.WritePunctuation(", ");

                arg.WriteTo(writer);
            }
            writer.WritePunctuation(")");
        }

        private static void WriteTypeCastExpression(BoundTypeCastExpression node, IndentedTextWriter writer)
        {
            writer.WriteKeyword(node.Type.Name);
            writer.WritePunctuation("(");
            node.Expression.WriteTo(writer);
            writer.WritePunctuation(")");
        }

        private static void WriteIndexingExpression(BoundIndexingExpression node, IndentedTextWriter writer)
        {
            writer.WriteIdentifier(node.List.Name);
            writer.WritePunctuation("[");
            node.Index.WriteTo(writer);
            writer.WritePunctuation("]");
        }

        private static void WriteListExpression(BoundListExpression node, IndentedTextWriter writer)
        {
            writer.WritePunctuation("[");

            var isFirst = false;
            foreach (var element in node.Contents)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.WritePunctuation(", ");

                element.WriteTo(writer);
            }

            writer.WritePunctuation("]");
        }

        private static void WriteErrorExpression(BoundErrorExpression node, IndentedTextWriter writer)
        {
            writer.WriteKeyword("ERROR");
            writer.WriteLine();
        }
    }
}