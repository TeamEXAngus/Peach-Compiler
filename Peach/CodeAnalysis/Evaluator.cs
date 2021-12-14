using System;
using System.Collections.Generic;
using System.Linq;
using Peach.CodeAnalysis.Binding;
using Peach.CodeAnalysis.Syntax;

namespace Peach.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundExpression _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        public Evaluator(BoundExpression root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.ParenthesisedExpression:
                    return EvaluateParenthesisedExpression(node as BoundParenthesisedExpression);

                case BoundNodeKind.LiteralExpression:
                    return EvaluateLiteralExpression(node as BoundLiteralExpresion);

                case BoundNodeKind.VariableExpression:
                    return EvaluateVariableExpression(node as BoundVariableExpression);

                case BoundNodeKind.AssignmentExpression:
                    return EvaluateAssignmentExpression(node as BoundAssignmentExpression);

                case BoundNodeKind.UnaryExpression:
                    return EvaluateUnaryExpression(node as BoundUnaryExpression);

                case BoundNodeKind.BinaryExpression:
                    return EvaluateBinaryExpression(node as BoundBinaryExpression);

                default:
                    throw new Exception($"Unexpected node '{node.Kind}'");
            }
        }

        private object EvaluateParenthesisedExpression(BoundParenthesisedExpression node)
        {
            return EvaluateExpression(node.Expression);
        }

        private object EvaluateLiteralExpression(BoundLiteralExpresion node)
        {
            return node.Value;
        }

        private object EvaluateVariableExpression(BoundVariableExpression node)
        {
            return _variables[node.Variable];
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression node)
        {
            var value = EvaluateExpression(node.Expression);
            _variables[node.Variable] = value;
            return value;
        }

        private object EvaluateUnaryExpression(BoundUnaryExpression node)
        {
            var operand = EvaluateExpression(node.Operand);

            return node.Op.Kind switch
            {
                BoundUnaryOperatorKind.Identity => (int)operand,
                BoundUnaryOperatorKind.Negation => -(int)operand,
                BoundUnaryOperatorKind.LogicalNot => !(bool)operand,
                _ => throw new Exception($"Unexpected unary operator '{node.Op.Kind}'"),
            };
        }

        private object EvaluateBinaryExpression(BoundBinaryExpression node)
        {
            var left = EvaluateExpression(node.Left);

            var right = EvaluateExpression(node.Right);

            return node.Op.Kind switch
            {
                BoundBinaryOperatorKind.Addition => (int)left + (int)right,
                BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
                BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
                BoundBinaryOperatorKind.Division => (int)left / (int)right,
                BoundBinaryOperatorKind.LogicalAnd => (bool)left && (bool)right,
                BoundBinaryOperatorKind.LogicalOr => (bool)left || (bool)right,
                BoundBinaryOperatorKind.Equality => Equals(left, right),
                BoundBinaryOperatorKind.Inequality => !Equals(left, right),
                _ => throw new Exception($"Unexpected binary operator '{node.Op.Kind}'"),
            };
        }
    }
}