using System;
using System.Collections.Generic;
using Peach.CodeAnalysis.Binding;

namespace Peach.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly BoundStatement _root;
        private readonly Dictionary<VariableSymbol, object> _variables;

        private object _lastValue;

        public Evaluator(BoundStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _root = root;
            _variables = variables;
        }

        public object Evaluate()
        {
            EvaluateStatement(_root);
            return _lastValue;
        }

        private void EvaluateStatement(BoundStatement node)
        {
            switch (node.Kind)
            {
                case BoundNodeKind.BlockStatement:
                    EvaluateBlockStatement(node as BoundBlockStatement);
                    break;

                case BoundNodeKind.VariableDeclaration:
                    EvaluateVariableDeclaration(node as BoundVariableDeclaration);
                    break;

                case BoundNodeKind.IfStatement:
                    EvaluateIfStatement(node as BoundIfStatement);
                    break;

                case BoundNodeKind.ExpressionStatement:
                    EvaluateExpressionStatement(node as BoundExpressionStatement);
                    break;

                default:
                    throw new Exception($"Unexpected statement {node.Kind}");
            }
        }

        private void EvaluateBlockStatement(BoundBlockStatement node)
        {
            foreach (var statement in node.Statements)
            {
                EvaluateStatement(statement);
            }
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _variables[node.Variable] = value;
            _lastValue = value;
        }

        private void EvaluateIfStatement(BoundIfStatement node)
        {
            var condition = (bool)EvaluateExpression(node.Condition);
            if (node.Negated)
                condition = !condition;
            if (condition)
                EvaluateStatement(node.ThenStatment);
            else if (node.ElseStatement is not null)
                EvaluateStatement(node.ElseStatement);
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
                BoundNodeKind.BlockStatement => EvaluateUnaryExpression(node as BoundUnaryExpression),
                BoundNodeKind.BinaryExpression => EvaluateBinaryExpression(node as BoundBinaryExpression),
                _ => throw new Exception($"Unexpected node '{node.Kind}'"),
            };
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
                BoundBinaryOperatorKind.LessThan => (int)left < (int)right,
                BoundBinaryOperatorKind.LessOrEqual => (int)left <= (int)right,
                BoundBinaryOperatorKind.GreaterThan => (int)left > (int)right,
                BoundBinaryOperatorKind.GreaterOrEqual => (int)left >= (int)right,
                _ => throw new Exception($"Unexpected binary operator '{node.Op.Kind}'"),
            };
        }
    }
}