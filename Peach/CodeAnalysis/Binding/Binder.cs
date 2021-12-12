using Peach.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new();
        public DiagnosticBag Diagnostics => _diagnostics;

        private readonly Dictionary<VariableSymbol, object> _variables;

        public Binder(Dictionary<VariableSymbol, object> variables)
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

            var variable = _variables.Keys.FirstOrDefault(v => v.Name == name);

            if (variable is null)
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundLiteralExpresion(0);
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;
            var boundExpression = BindExpression(syntax.Expression);

            var existingVariable = _variables.Keys.FirstOrDefault(v => v.Name == name);

            if (existingVariable is not null)
                _variables.Remove(existingVariable);

            var variable = new VariableSymbol(name, boundExpression.Type);
            _variables[variable] = null;

            return new BoundAssignmentExpression(variable, boundExpression);
        }
    }
}