using System;
using System.Collections.Generic;
using Peach.CodeAnalysis.Syntax;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly List<string> _diagnostics = new();
        public IEnumerable<string> Diagnostics => _diagnostics;

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
            }

            throw new Exception("$Unexpected syntax {syntax.Kind}");
        }

        private BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpresion(value);
        }

        private BoundExpression BindUnaryExpression(UnaryExpressionSyntax syntax)
        {
            var boundOperand = BindExpression(syntax.Operand);
            var boundOperatorKind = BindUnaryOperatorKind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundOperatorKind is null)
            {
                _diagnostics.Add($"Unary operator {syntax.OperatorToken.Text} is not defined for type {boundOperand.Type}");
                return boundOperand;
            }

            return new BoundUnaryExpression(boundOperatorKind.Value, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);
            var boundOperatorKind = BindBinaryOperatorKind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperatorKind is null)
            {
                _diagnostics.Add($"Unary operator {syntax.OperatorToken.Text} is not defined for types {boundLeft.Type}, {boundRight.Type}");
                return boundLeft;
            }

            return new BoundBinaryExpression(boundLeft, boundOperatorKind.Value, boundRight);
        }

        private BoundUnaryOperatorKind? BindUnaryOperatorKind(SyntaxKind kind, Type operandType)
        {
            if (operandType == typeof(int))

                switch (kind)
                {
                    case SyntaxKind.PlusToken:
                        return BoundUnaryOperatorKind.Identity;

                    case SyntaxKind.MinusToken:
                        return BoundUnaryOperatorKind.Negation;
                }

            if (operandType == typeof(bool))

                switch (kind)
                {
                    case SyntaxKind.ExclamationToken:
                        return BoundUnaryOperatorKind.LogicalNot;
                }

            return null;
        }

        private BoundBinaryOperatorKind? BindBinaryOperatorKind(SyntaxKind kind, Type leftType, Type rightType)
        {
            if (leftType == typeof(int) && rightType == typeof(int))

                switch (kind)
                {
                    case SyntaxKind.PlusToken:
                        return BoundBinaryOperatorKind.Addition;

                    case SyntaxKind.MinusToken:
                        return BoundBinaryOperatorKind.Subtraction;

                    case SyntaxKind.AsteriskToken:
                        return BoundBinaryOperatorKind.Multiplication;

                    case SyntaxKind.SlashToken:
                        return BoundBinaryOperatorKind.Division;
                }

            if (leftType == typeof(bool) && rightType == typeof(bool))

                switch (kind)
                {
                    case SyntaxKind.AmpersandAmpersandToken:
                        return BoundBinaryOperatorKind.LogicalAnd;

                    case SyntaxKind.PipePipeToken:
                        return BoundBinaryOperatorKind.LogicalOr;
                }

            return null;
        }
    }
}