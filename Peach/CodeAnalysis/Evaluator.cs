using System;
using System.Collections.Generic;
using Peach.CodeAnalysis.Binding;
using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        private object _lastValue;

        public Evaluator(BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (int i = 0; i < _root.Statements.Length; i++)
            {
                var statement = _root.Statements[i];

                if (statement is BoundLabelStatement l)
                    labelToIndex.Add(l.Label, i + 1);
            }

            var index = 0;
            while (index < _root.Statements.Length)
            {
                var s = _root.Statements[index];
                switch (s.Kind)
                {
                    case BoundNodeKind.VariableDeclaration:
                        EvaluateVariableDeclaration(s as BoundVariableDeclaration);
                        index++;
                        break;

                    case BoundNodeKind.ExpressionStatement:
                        EvaluateExpressionStatement(s as BoundExpressionStatement);
                        index++;
                        break;

                    case BoundNodeKind.GotoStatement:
                        index = labelToIndex[(s as BoundGotoStatement).Label];
                        break;

                    case BoundNodeKind.ConditionalGotoStatement:
                        var _this = s as BoundConditionalGotoStatement;
                        var condition = (bool)EvaluateExpression(_this.Condition);
                        if (condition == _this.JumpIfTrue)
                            index = labelToIndex[_this.Label];
                        else
                            index++;
                        break;

                    case BoundNodeKind.LabelStatement:
                        index++;
                        break;

                    default:
                        throw new Exception($"Unexpected statement {s.Kind}");
                }
            }

            return _lastValue;
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
        }

        private void EvaluateExpressionStatement(BoundExpressionStatement node)
        {
            _lastValue = EvaluateExpression(node.Expression);
        }

        private object EvaluateExpression(BoundExpression node)
        {
            return node.Kind switch
            {
                BoundNodeKind.ParenthesisedExpression => EvaluateParenthesisedExpression(node as BoundParenthesisedExpression),
                BoundNodeKind.LiteralExpression => EvaluateLiteralExpression(node as BoundLiteralExpresion),
                BoundNodeKind.VariableExpression => EvaluateVariableExpression(node as BoundVariableExpression),
                BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression(node as BoundAssignmentExpression),
                BoundNodeKind.UnaryExpression => EvaluateUnaryExpression(node as BoundUnaryExpression),
                BoundNodeKind.BinaryExpression => EvaluateBinaryExpression(node as BoundBinaryExpression),
                _ => throw new Exception($"Unexpected node in {nameof(EvaluateExpression)} '{node.Kind}'"),
            };
        }

        private object EvaluateParenthesisedExpression(BoundParenthesisedExpression node)
        {
            return EvaluateExpression(node.Expression);
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpresion node)
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
                BoundUnaryOperatorKind.BitwiseNot => ~(int)operand,
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
                BoundBinaryOperatorKind.LessThan => (int)left < (int)right,
                BoundBinaryOperatorKind.LessOrEqual => (int)left <= (int)right,
                BoundBinaryOperatorKind.GreaterThan => (int)left > (int)right,
                BoundBinaryOperatorKind.GreaterOrEqual => (int)left >= (int)right,
                BoundBinaryOperatorKind.BitwiseAnd => EvaluateBitwiseAnd(left, right),
                BoundBinaryOperatorKind.BitwiseOr => EvaluateBitwiseOr(left, right),
                BoundBinaryOperatorKind.BitwiseXor => EvaluateBitwiseXor(left, right),
                _ => throw new Exception($"Unexpected binary operator '{node.Op.Kind}'"),
            };
        }

        private static object EvaluateBitwiseAnd(object left, object right)
        {
            if (left is int L && right is int R)
                return L & R;
            else if (left is bool Lb && right is bool Rb)
                return Lb & Rb;
            throw new Exception($"Invalid operand types {left.GetType()} and {right.GetType()}");
        }

        private static object EvaluateBitwiseOr(object left, object right)
        {
            if (left is int L && right is int R)
                return L | R;
            else if (left is bool Lb && right is bool Rb)
                return Lb | Rb;
            throw new Exception($"Invalid operand types {left.GetType()} and {right.GetType()}");
        }

        private static object EvaluateBitwiseXor(object left, object right)
        {
            if (left is int L && right is int R)
                return L ^ R;
            else if (left is bool Lb && right is bool Rb)
                return Lb ^ Rb;
            throw new Exception($"Invalid operand types {left.GetType()} and {right.GetType()}");
        }
    }
}