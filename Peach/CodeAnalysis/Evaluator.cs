using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Peach.CodeAnalysis.Binding;
using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis
{
    internal sealed class Evaluator
    {
        private readonly ImmutableDictionary<FunctionSymbol, BoundBlockStatement> _functionBodies;
        private readonly BoundBlockStatement _root;
        private readonly Dictionary<VariableSymbol, object> _globals;
        private readonly Stack<Dictionary<VariableSymbol, object>> _locals = new();

        private object _lastValue;

        public Evaluator(ImmutableDictionary<FunctionSymbol, BoundBlockStatement> functionBodies, BoundBlockStatement root, Dictionary<VariableSymbol, object> variables)
        {
            _functionBodies = functionBodies;
            _root = root;
            _globals = variables;
        }

        public object Evaluate()
        {
            var body = _root;

            return EvaluateStatement(body);
        }

        private object EvaluateStatement(BoundBlockStatement body)
        {
            var labelToIndex = new Dictionary<BoundLabel, int>();

            for (int i = 0; i < body.Statements.Length; i++)
            {
                var statement = body.Statements[i];

                if (statement is BoundLabelStatement l)
                    labelToIndex.Add(l.Label, i + 1);
            }

            var index = 0;
            while (index < body.Statements.Length)
            {
                var s = body.Statements[index];

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

                    case BoundNodeKind.ReturnStatement:
                        var statement = s as BoundReturnStatement;
                        var val = statement.Expression is null ? null : EvaluateExpression(statement.Expression);
                        return val;

                    default:
                        throw new Exception($"Unexpected statement {s.Kind}");
                }
            }

            return _lastValue;
        }

        private void EvaluateVariableDeclaration(BoundVariableDeclaration node)
        {
            var value = EvaluateExpression(node.Initializer);
            _lastValue = value;

            Assign(node.Variable, value);
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
                BoundNodeKind.LiteralExpression => EvaluateLiteralExpression(node as BoundLiteralExpression),
                BoundNodeKind.VariableExpression => EvaluateVariableExpression(node as BoundVariableExpression),
                BoundNodeKind.AssignmentExpression => EvaluateAssignmentExpression(node as BoundAssignmentExpression),
                BoundNodeKind.UnaryExpression => EvaluateUnaryExpression(node as BoundUnaryExpression),
                BoundNodeKind.BinaryExpression => EvaluateBinaryExpression(node as BoundBinaryExpression),
                BoundNodeKind.FunctionCallExpression => EvaluateFunctionCallExpression(node as BoundFunctionCallExpression),
                BoundNodeKind.TypeCastExpression => EvaluateTypeCastExpression(node as BoundTypeCastExpression),
                _ => throw new Exception($"Unexpected node in {nameof(EvaluateExpression)} '{node.Kind}'"),
            };
        }

        private object EvaluateParenthesisedExpression(BoundParenthesisedExpression node)
        {
            return EvaluateExpression(node.Expression);
        }

        private static object EvaluateLiteralExpression(BoundLiteralExpression node)
        {
            return node.Value;
        }

        private object EvaluateVariableExpression(BoundVariableExpression node)
        {
            return AccessVariable(node.Variable);
        }

        private object AccessVariable(VariableSymbol variable)
        {
            if (variable.Kind == SymbolKind.GlobalVariable)
            {
                return _globals[variable];
            }

            var locals = _locals.Peek();
            return locals[variable];
        }

        private object EvaluateAssignmentExpression(BoundAssignmentExpression node)
        {
            var value = EvaluateExpression(node.Expression);

            Assign(node.Variable, value);

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
                BoundBinaryOperatorKind.StringAddition => string.Concat(left as string, right as string),
                BoundBinaryOperatorKind.Subtraction => (int)left - (int)right,
                BoundBinaryOperatorKind.Multiplication => (int)left * (int)right,
                BoundBinaryOperatorKind.Division => (int)left / (int)right,
                BoundBinaryOperatorKind.Modulo => (int)left % (int)right,
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

        private static Random Rand = null;

        private object EvaluateFunctionCallExpression(BoundFunctionCallExpression node)
        {
            if (node.Function == BuiltinFunctions.Input)
                return Console.ReadLine();

            if (node.Function == BuiltinFunctions.Print)
            {
                Console.WriteLine(EvaluateExpression(node.Arguments[0]));
                return null;
            }

            if (node.Function == BuiltinFunctions.Rand)
            {
                if (Rand is null)
                    Rand = new Random();

                return Rand.Next(int.MinValue, int.MaxValue);
            }

            if (node.Function == BuiltinFunctions.RandRange)
            {
                if (Rand is null)
                    Rand = new Random();

                var lower = (int)EvaluateExpression(node.Arguments[0]);
                var upper = (int)EvaluateExpression(node.Arguments[1]);

                return Rand.Next(lower, upper);
            }

            var locals = new Dictionary<VariableSymbol, object>();
            for (int i = 0; i < node.Arguments.Length; i++)
            {
                var param = node.Function.Parameters[i];
                var value = EvaluateExpression(node.Arguments[i]);
                locals.Add(param, value);
            }
            _locals.Push(locals);

            var statement = _functionBodies[node.Function];
            var result = EvaluateStatement(statement);

            _locals.Pop();

            return result;
        }

        private object EvaluateTypeCastExpression(BoundTypeCastExpression node)
        {
            var from = node.Expression.Type.TypeID;
            var to = node.Type.TypeID;
            return (from, to) switch
            {
                (from: TypeID.Int, to: TypeID.String) => EvaluateExpression(node.Expression).ToString(),
                (from: TypeID.Bool, to: TypeID.String) => EvaluateExpression(node.Expression).ToString(),
                (from: TypeID.String, to: TypeID.Int) => IntFromString((string)EvaluateExpression(node.Expression)),
                (from: TypeID.String, to: TypeID.Bool) => BoolFromString((string)EvaluateExpression(node.Expression)),
                (_, _) => throw new Exception($"No conversion exists from {from} to {to}"),
            };
        }

        private static int IntFromString(string str)
        {
            if (int.TryParse(str, out var result))
                return result;

            return int.MinValue;
        }

        private static bool BoolFromString(string str)
        {
            if (bool.TryParse(str, out var result))
                return result;

            return default;
        }

        private void Assign(VariableSymbol variable, object value)
        {
            if (variable.Kind == SymbolKind.GlobalVariable)
            {
                _globals[variable] = value;
                return;
            }

            var locals = _locals.Peek();
            locals[variable] = value;
        }
    }
}