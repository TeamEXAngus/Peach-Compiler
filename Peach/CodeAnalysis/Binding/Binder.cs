using Peach.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public string Name { get; }
        public override Type Type { get; }
    }

    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new();
        public DiagnosticBag Diagnostics => _diagnostics;

        private readonly Dictionary<string, object> _variables;

        public Binder(Dictionary<string, object> variables)
        {
            _variables = variables;
        }

        public BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            switch (syntax.Kind)
            {
                case SyntaxKind.LiteralExpression:
                    return BindLiteralExpression(syntax as LiteralExpressionSyntax);

                case SyntaxKind.UnaryExpression:
                    return BindUnaryExpression(syntax as UnaryExpressionSyntax);

                case SyntaxKind.BinaryExpression:
                    return BindBinaryExpression(syntax as BinaryExpressionSyntax);

                case SyntaxKind.ParenthesisedExpression:
                    return BindParenthesisedExpression(syntax as ParenthesisedExpressionSyntax);

                case SyntaxKind.NameExpression:
                    return BindNameExpression(syntax as NameExpressionSyntax);

                case SyntaxKind.AssignmentExpression:
                    return BindAssignmentExpression(syntax as AssignmentExpressionSyntax);
            }

            throw new Exception($"Unexpected syntax {syntax.Kind}");
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpresion(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperatorKind = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundOperatorKind is null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return boundOperand;
            }

            return new BoundUnaryExpression(boundOperatorKind, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            var boundOperatorKind = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperatorKind is null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return boundLeft;
            }

            return new BoundBinaryExpression(boundLeft, boundOperatorKind, boundRight);
        }

        private BoundExpression BindParenthesisedExpression(ParenthesisedExpressionSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression);

            return new BoundParenthesisedExpression(expression);
        }

        private BoundExpression BindNameExpression(NameExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            if (!_variables.TryGetValue(name, out var value))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpresion(0);
            }

            var type = value?.GetType() ?? typeof(object);
            return new BoundVariableExpression(name, type);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            object defaultValue =
                boundExpression.Type == typeof(int)
                    ? 0
                    : boundExpression.Type == typeof(bool)
                        ? false
                        : null;

            if (defaultValue is null)
                throw new Exception($"Unsuported variable type: {boundExpression.Type}");

            _variables[name] = defaultValue;

            return new BoundAssignmentExpression(name, boundExpression);
        }
    }
}