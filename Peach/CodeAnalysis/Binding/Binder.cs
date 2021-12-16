using Peach.CodeAnalysis.Symbols;
using Peach.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class Binder
    {
        private readonly DiagnosticBag _diagnostics = new();
        private BoundScope _scope;

        public Binder(BoundScope parent)
        {
            _scope = new BoundScope(parent);
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScopes(previous);
            var binder = new Binder(parentScope);
            var expression = binder.BindStatement(syntax.Statement);
            var variables = binder._scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous is not null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, variables, expression);
        }

        private static BoundScope CreateParentScopes(BoundGlobalScope previous)
        {
            var stack = new Stack<BoundGlobalScope>();

            while (previous is not null)
            {
                stack.Push(previous);
                previous = previous.Previous;
            }

            BoundScope parent = null;

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var v in previous.Variables)
                    scope.TryDeclare(v);

                parent = scope;
            }

            return parent;
        }

        public DiagnosticBag Diagnostics => _diagnostics;

        private BoundStatement BindStatement(StatementSyntax syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.BlockStatement => BindBlockStatement(syntax as BlockStatementSyntax),
                SyntaxKind.VariableDeclaration => BindVariableDeclaration(syntax as VariableDeclarationSyntax),
                SyntaxKind.IfStatement => BindIfStatement(syntax as IfStatementSyntax),
                SyntaxKind.WhileStatement => BindWhileStatement(syntax as WhileStatementSyntax),
                SyntaxKind.LoopStatement => BindLoopStatement(syntax as LoopStatementSyntax),
                SyntaxKind.ForStatement => BindForStatement(syntax as ForStatementSyntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement(syntax as ExpressionStatementSyntax),
                _ => throw new Exception($"Unexpected statement {syntax.Kind}"),
            };
        }

        private BoundStatement BindBlockStatement(BlockStatementSyntax syntax)
        {
            var statements = ImmutableArray.CreateBuilder<BoundStatement>();
            _scope = new BoundScope(_scope);

            foreach (var statementSyntax in syntax.Statements)
            {
                var statement = BindStatement(statementSyntax);
                statements.Add(statement);
            }

            _scope = _scope.Parent;

            return new BoundBlockStatement(statements.ToImmutable());
        }

        private BoundStatement BindVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            var name = syntax.Identifier.Text;
            var isConst = syntax.Keyword.Kind == SyntaxKind.ConstKeyword;
            var initializer = BindExpression(syntax.Initializer);
            var variable = new VariableSymbol(name, isConst, initializer.Type);

            if (!_scope.TryDeclare(variable))
            {
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Identifier.Span, name);
            }

            return new BoundVariableDeclaration(variable, initializer);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, typeof(bool));
            var statement = BindStatement(syntax.ThenStatement);
            var elseClause = syntax.ElseClause is null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, syntax.IsNegated, statement, elseClause);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, typeof(bool));
            var body = BindStatement(syntax.Body);
            return new BoundWhileStatement(condition, syntax.IsNegated, body);
        }

        private BoundStatement BindLoopStatement(LoopStatementSyntax syntax)
        {
            var body = BindStatement(syntax.Body);
            return new BoundLoopStatement(body);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var start = BindExpression(syntax.Start, typeof(int));
            var stop = BindExpression(syntax.Stop, typeof(int));
            var step = BindExpression(syntax.Step, typeof(int));

            _scope = new BoundScope(_scope);

            var name = syntax.Variable.Text;
            var variable = new VariableSymbol(name, true, typeof(int));
            if (!_scope.TryDeclare(variable))
                _diagnostics.ReportVariableAlreadyDeclared(syntax.Variable.Span, name);

            var body = BindStatement(syntax.Body);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, start, stop, step, body);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression);

            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, Type expectedType)
        {
            var result = BindExpression(syntax);
            if (result.Type != expectedType)
                _diagnostics.ReportCannotConvertTypes(syntax.Span, result.Type, expectedType);

            return result;
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.LiteralExpression => BindLiteralExpression(syntax as LiteralExpressionSyntax),
                SyntaxKind.UnaryExpression => BindUnaryExpression(syntax as UnaryExpressionSyntax),
                SyntaxKind.BinaryExpression => BindBinaryExpression(syntax as BinaryExpressionSyntax),
                SyntaxKind.ParenthesisedExpression => BindParenthesisedExpression(syntax as ParenthesisedExpressionSyntax),
                SyntaxKind.NameExpression => BindNameExpression(syntax as NameExpressionSyntax),
                SyntaxKind.AssignmentExpression => BindAssignmentExpression(syntax as AssignmentExpressionSyntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
            };
        }

        // does not access instance data
        private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
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

            if (string.IsNullOrEmpty(name))          // Token inserted by parser
                return new BoundLiteralExpresion(0);

            if (!_scope.TryLookup(name, out var variable))
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

            if (!_scope.TryLookup(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return boundExpression;
            }

            if (variable.IsConst)
            {
                _diagnostics.ReportCannotAssignToConst(syntax.EqualsToken.Span, name);
            }

            if (boundExpression.Type != variable.Type)
            {
                _diagnostics.ReportCannotConvertTypes(syntax.Expression.Span, boundExpression.Type, variable.Type);
                return boundExpression;
            }

            return new BoundAssignmentExpression(variable, boundExpression);
        }
    }
}