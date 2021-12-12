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
        private readonly Dictionary<string, object> _variables;

        public Evaluator(BoundExpression root, Dictionary<string, object> variables)
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
            if (node is BoundLiteralExpresion n)
                return n.Value;

            if (node is BoundVariableExpression v)
                return _variables[v.Name];

            if (node is BoundAssignmentExpression a)
            {
                var value = EvaluateExpression(a.Expression);
                _variables[a.Name] = value;
                return value;
            }

            if (node is BoundUnaryExpression u)
            {
                var operand = EvaluateExpression(u.Operand);

                switch (u.Op.Kind)
                {
                    case BoundUnaryOperatorKind.Identity:
                        return (int)operand;

                    case BoundUnaryOperatorKind.Negation:
                        return -(int)operand;

                    case BoundUnaryOperatorKind.LogicalNot:
                        return !(bool)operand;
                }

                throw new Exception($"Unexpected unary operator '{u.Op.Kind}'");
            }

            if (node is BoundBinaryExpression b)

            {
                var left = EvaluateExpression(b.Left);

                var right = EvaluateExpression(b.Right);

                switch (b.Op.Kind)
                {
                    case BoundBinaryOperatorKind.Addition:
                        return (int)left + (int)right;

                    case BoundBinaryOperatorKind.Subtraction:
                        return (int)left - (int)right;

                    case BoundBinaryOperatorKind.Multiplication:
                        return (int)left * (int)right;

                    case BoundBinaryOperatorKind.Division:
                        return (int)left / (int)right;

                    case BoundBinaryOperatorKind.LogicalAnd:
                        return (bool)left && (bool)right;

                    case BoundBinaryOperatorKind.LogicalOr:
                        return (bool)left || (bool)right;

                    case BoundBinaryOperatorKind.Equality:
                        return Equals(left, right);

                    case BoundBinaryOperatorKind.Inequality:
                        return !Equals(left, right);
                }

                throw new Exception($"Unexpected binary operator '{b.Op.Kind}'");
            }

            if (node is BoundParenthesisedExpression p)
                return EvaluateExpression(p.Expression);

            throw new Exception($"Unexpected node '{node.Kind}'");
        }
    }
}