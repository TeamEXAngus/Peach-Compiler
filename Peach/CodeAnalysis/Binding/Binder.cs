using Peach.CodeAnalysis.Symbols;
using Peach.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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

            var parent = CreateRootScope();

            while (stack.Count > 0)
            {
                previous = stack.Pop();
                var scope = new BoundScope(parent);
                foreach (var v in previous.Variables)
                    scope.TryDeclareVariable(v);

                parent = scope;
            }

            return parent;
        }

        private static BoundScope CreateRootScope()
        {
            var result = new BoundScope(null);

            foreach (var f in BuiltinFunctions.GetAll())
            {
                result.TryDeclareFunction(f);
            }

            return result;
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
            var isConst = syntax.Keyword.Kind == SyntaxKind.ConstKeyword;
            var initializer = BindExpression(syntax.Initializer);
            var variable = BindVariable(syntax.Identifier, isConst, initializer.Type);

            return new BoundVariableDeclaration(variable, initializer);
        }

        private BoundStatement BindIfStatement(IfStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
            var statement = BindStatement(syntax.ThenStatement);
            var elseClause = syntax.ElseClause is null ? null : BindStatement(syntax.ElseClause.ElseStatement);
            return new BoundIfStatement(condition, syntax.IsNegated, statement, elseClause);
        }

        private BoundStatement BindWhileStatement(WhileStatementSyntax syntax)
        {
            var condition = BindExpression(syntax.Condition, TypeSymbol.Bool);
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
            var start = BindExpression(syntax.Start, TypeSymbol.Int);
            var stop = BindExpression(syntax.Stop, TypeSymbol.Int);
            var step = BindExpression(syntax.Step, TypeSymbol.Int);

            _scope = new BoundScope(_scope);

            var variable = BindVariable(syntax.Variable, isConst: true, TypeSymbol.Int);

            var body = BindStatement(syntax.Body);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, start, stop, step, body);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);

            return new BoundExpressionStatement(expression);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, TypeSymbol expectedType)
        {
            return BindTypeCastExpression(syntax, expectedType);
        }

        private BoundExpression BindExpression(ExpressionSyntax syntax, bool canBeVoid = false)
        {
            var result = BindRawExpression(syntax);

            if (!canBeVoid && result.Type == TypeSymbol.Void)
            {
                _diagnostics.ReportExpressionCannotBeVoid(syntax.Span);
                return new BoundErrorExpression();
            }

            return result;
        }

        private BoundExpression BindRawExpression(ExpressionSyntax syntax)
        {
            return syntax.Kind switch
            {
                SyntaxKind.LiteralExpression => BindLiteralExpression(syntax as LiteralExpressionSyntax),
                SyntaxKind.UnaryExpression => BindUnaryExpression(syntax as UnaryExpressionSyntax),
                SyntaxKind.BinaryExpression => BindBinaryExpression(syntax as BinaryExpressionSyntax),
                SyntaxKind.ParenthesisedExpression => BindParenthesisedExpression(syntax as ParenthesisedExpressionSyntax),
                SyntaxKind.NameExpression => BindNameExpression(syntax as NameExpressionSyntax),
                SyntaxKind.AssignmentExpression => BindAssignmentExpression(syntax as AssignmentExpressionSyntax),
                SyntaxKind.FunctionCallExpression => BindFunctionCallExpression(syntax as FunctionCallExpressionSyntax),
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

            if (boundOperand.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var boundOperatorKind = BoundUnaryOperator.Bind(syntax.OperatorToken.Kind, boundOperand.Type);

            if (boundOperatorKind is null)
            {
                _diagnostics.ReportUndefinedUnaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundOperand.Type);
                return new BoundErrorExpression();
            }

            return new BoundUnaryExpression(boundOperatorKind, boundOperand);
        }

        private BoundExpression BindBinaryExpression(BinaryExpressionSyntax syntax)
        {
            var boundLeft = BindExpression(syntax.Left);
            var boundRight = BindExpression(syntax.Right);

            if (boundLeft.Type == TypeSymbol.Error || boundRight.Type == TypeSymbol.Error)
                return new BoundErrorExpression();

            var boundOperatorKind = BoundBinaryOperator.Bind(syntax.OperatorToken.Kind, boundLeft.Type, boundRight.Type);

            if (boundOperatorKind is null)
            {
                _diagnostics.ReportUndefinedBinaryOperator(syntax.OperatorToken.Span, syntax.OperatorToken.Text, boundLeft.Type, boundRight.Type);
                return new BoundErrorExpression();
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
                return new BoundErrorExpression();

            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return new BoundErrorExpression();
            }

            return new BoundVariableExpression(variable);
        }

        private BoundExpression BindAssignmentExpression(AssignmentExpressionSyntax syntax)
        {
            var name = syntax.IdentifierToken.Text;

            if (!_scope.TryLookupVariable(name, out var variable))
            {
                _diagnostics.ReportUndefinedName(syntax.IdentifierToken.Span, name);
                return BindExpression(syntax.Expression);
            }

            if (variable.IsConst)
            {
                _diagnostics.ReportCannotAssignToConst(syntax.EqualsToken.Span, name);
            }

            var boundExpression = BindExpression(syntax.Expression, variable.Type);

            return new BoundAssignmentExpression(variable, boundExpression);
        }

        private BoundExpression BindFunctionCallExpression(FunctionCallExpressionSyntax syntax)
        {
            var argBuilder = ImmutableArray.CreateBuilder<BoundExpression>();

            if (TypeSymbol.LookupTypeFromText(syntax.Identifier.Text) is TypeSymbol t)
            {
                if (syntax.Arguments.Count != 1)
                {
                    _diagnostics.ReportWrongNumberOfArguments(syntax.Span, syntax.Arguments.Count, 1);
                    return new BoundErrorExpression();
                }

                return BindTypeCastExpression(syntax.Arguments[0], t);
            }

            foreach (var arg in syntax.Arguments)
            {
                var boundArg = BindExpression(arg);
                argBuilder.Add(boundArg);
            }

            var boundArguments = argBuilder.ToImmutable();

            if (!_scope.TryLookupFunction(syntax.Identifier.Text, out var function))
            {
                _diagnostics.ReportUndefinedFunction(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            if (syntax.Arguments.Count != function.Parameters.Length)
            {
                _diagnostics.ReportWrongNumberOfArguments(syntax.Span, syntax.Arguments.Count, function.Parameters.Length);
                return new BoundErrorExpression();
            }

            for (int i = 0; i < syntax.Arguments.Count; i++)
            {
                var arg = boundArguments[i];
                var param = function.Parameters[i];

                if (arg.Type != param.Type)
                {
                    if (arg.Type != TypeSymbol.Error)
                    {
                        _diagnostics.ReportIncorrectArgumentType(syntax.Arguments[i].Span, arg.Type, param.Type);
                    }

                    return new BoundErrorExpression();
                }
            }

            return new BoundFunctionCallExpression(function, boundArguments);
        }

        private BoundExpression BindTypeCastExpression(ExpressionSyntax syntax, TypeSymbol type)
        {
            var expression = BindExpression(syntax);

            var conversion = TypeCasting.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                    _diagnostics.ReportCannotConvertTypes(syntax.Span, expression.Type, type);

                return new BoundErrorExpression();
            }

            if (conversion.IsIdentity)
                return expression;

            return new BoundTypeCastExpression(type, expression);
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, bool isConst, TypeSymbol type)
        {
            var name = identifier.Text ?? "<error>";
            var shouldDeclare = !identifier.IsMissing;

            var variable = new VariableSymbol(name, isConst, type);
            if (shouldDeclare && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportVariableAlreadyDeclared(identifier.Span, identifier.Text);
            return variable;
        }
    }
}