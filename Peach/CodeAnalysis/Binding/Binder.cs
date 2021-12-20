using Peach.CodeAnalysis.Lowering;
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
        private readonly FunctionSymbol _function;
        private BoundScope _scope;

        private readonly Stack<(BoundLabel breakLabel, BoundLabel continueLabel)> _loopStack = new();

        public Binder(BoundScope parent, FunctionSymbol function)
        {
            _scope = new BoundScope(parent);
            _function = function;

            if (function is not null)
            {
                foreach (var p in function.Parameters)
                {
                    _scope.TryDeclareVariable(p);
                }
            }
        }

        public static BoundGlobalScope BindGlobalScope(BoundGlobalScope previous, CompilationUnitSyntax syntax)
        {
            var parentScope = CreateParentScope(previous);
            var binder = new Binder(parentScope, null);

            foreach (var func in syntax.Members.OfType<FunctionDeclarationSyntax>())
                binder.BindFunctionDeclaration(func);

            var statements = ImmutableArray.CreateBuilder<BoundStatement>();

            foreach (var globalStatement in syntax.Members.OfType<GlobalStatementSyntax>())
                statements.Add(binder.BindStatement(globalStatement.Statement));

            var functions = binder._scope.GetDeclaredFunctions();
            var variables = binder._scope.GetDeclaredVariables();
            var diagnostics = binder.Diagnostics.ToImmutableArray();

            if (previous is not null)
                diagnostics = diagnostics.InsertRange(0, previous.Diagnostics);

            return new BoundGlobalScope(previous, diagnostics, functions, variables, statements.ToImmutable());
        }

        public static BoundProgram BindProgram(BoundGlobalScope globalScope)
        {
            var parentScope = CreateParentScope(globalScope);

            var functionBodies = ImmutableDictionary.CreateBuilder<FunctionSymbol, BoundBlockStatement>();

            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            var scope = globalScope;
            while (scope is not null)
            {
                foreach (var function in scope.Functions)
                {
                    var binder = new Binder(parentScope, function);
                    var body = binder.BindStatement(function.Declaration.Body);
                    var loweredBody = Lowerer.Lower(body);
                    functionBodies.Add(function, loweredBody);

                    diagnostics.AddRange(binder.Diagnostics);
                }
                scope = scope.Previous;
            }

            var statement = Lowerer.Lower(new BoundBlockStatement(globalScope.Statements));
            return new BoundProgram(diagnostics.ToImmutable(), functionBodies.ToImmutable(), statement);
        }

        private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax)
        {
            var builder = ImmutableArray.CreateBuilder<ParameterSymbol>();

            var seenParamNames = new HashSet<string>();

            foreach (var paramSyntax in syntax.Parameters)
            {
                var paramName = paramSyntax.Identifier.Text;
                var paramType = BindTypeClause(paramSyntax.TypeClause);
                if (!seenParamNames.Add(paramName))
                {
                    _diagnostics.ReportParameterAlreadyDeclared(paramSyntax.Span, paramName);
                }
                else
                {
                    var parameter = new ParameterSymbol(paramName, paramType);
                    builder.Add(parameter);
                }
            }

            var type = BindTypeClause(syntax.TypeClause) ?? TypeSymbol.Void;

            var function = new FunctionSymbol(syntax.Identifier.Text, builder.ToImmutable(), type, syntax);
            if (!_scope.TryDeclareFunction(function))
            {
                _diagnostics.ReportSymbolAlreadyDeclared(syntax.Identifier.Span, syntax.Identifier.Text);
            }
        }

        private static BoundScope CreateParentScope(BoundGlobalScope previous)
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

                foreach (var f in previous.Functions)
                    scope.TryDeclareFunction(f);

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
                SyntaxKind.ContinueStatement => BindContinueStatement(syntax as ContinueStatementSyntax),
                SyntaxKind.BreakStatement => BindBreakStatement(syntax as BreakStatementSyntax),
                SyntaxKind.ExpressionStatement => BindExpressionStatement(syntax as ExpressionStatementSyntax),
                SyntaxKind.ReturnStatement => BindReturnStatement(syntax as ReturnStatementSyntax),
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
            var type = BindTypeClause(syntax.TypeClause);
            var initializer = BindExpression(syntax.Initializer);
            var variableType = type ?? initializer.Type;
            var convertedInitializer = BindExpression(syntax.Initializer, variableType);
            var variable = BindVariable(syntax.Identifier, isConst, variableType);

            return new BoundVariableDeclaration(variable, convertedInitializer);
        }

        private TypeSymbol BindTypeClause(TypeClauseSyntax syntax)
        {
            if (syntax is null)
                return null;

            return BindType(syntax.Type);
        }

        private TypeSymbol BindType(TypeSyntax syntax)
        {
            if (syntax is TypeNameSyntax t)
                return BindTypeName(t);

            if (syntax is ListTypeSyntax l)
                return BindListType(l);

            throw new Exception($"Unknown type type {syntax}");
        }

        private TypeSymbol BindTypeName(TypeNameSyntax syntax)
        {
            var type = TypeSymbol.LookupTypeFromText(syntax.Identifier.Text);

            if (type is null)
                _diagnostics.ReportUndefinedType(syntax.Span, syntax.Identifier.Text);

            return type;
        }

        private TypeSymbol BindListType(ListTypeSyntax syntax)
        {
            var heldType = BindType(syntax.Type);

            var type = ListTypeSymbol.GetOrGenerateListTypeSymbol(heldType);

            return type;
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
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundWhileStatement(condition, syntax.IsNegated, body, breakLabel, continueLabel);
        }

        private BoundStatement BindLoopStatement(LoopStatementSyntax syntax)
        {
            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);
            return new BoundLoopStatement(body, breakLabel, continueLabel);
        }

        private BoundStatement BindForStatement(ForStatementSyntax syntax)
        {
            var start = BindExpression(syntax.Start, TypeSymbol.Int);
            var stop = BindExpression(syntax.Stop, TypeSymbol.Int);
            var step = BindExpression(syntax.Step, TypeSymbol.Int);

            _scope = new BoundScope(_scope);

            var variable = BindVariable(syntax.Variable, isConst: true, TypeSymbol.Int);

            var body = BindLoopBody(syntax.Body, out var breakLabel, out var continueLabel);

            _scope = _scope.Parent;

            return new BoundForStatement(variable, start, stop, step, body, breakLabel, continueLabel);
        }

        private BoundStatement BindLoopBody(StatementSyntax body, out BoundLabel breakLabel, out BoundLabel continueLabel)
        {
            _loopStack.Push((breakLabel, continueLabel) =
                            (BoundLabel.GenerateLabel("break"), BoundLabel.GenerateLabel("continue")));

            var boundBody = BindStatement(body);

            _loopStack.Pop();

            return boundBody;
        }

        private BoundStatement BindContinueStatement(ContinueStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportBreakContinueOutsideLoop(syntax.Span);
                return BindErrorStatement();
            }

            var continueLabel = _loopStack.Peek().continueLabel;

            return new BoundGotoStatement(continueLabel);
        }

        private BoundStatement BindBreakStatement(BreakStatementSyntax syntax)
        {
            if (_loopStack.Count == 0)
            {
                _diagnostics.ReportBreakContinueOutsideLoop(syntax.Span);
                return BindErrorStatement();
            }

            var breakLabel = _loopStack.Peek().breakLabel;

            return new BoundGotoStatement(breakLabel);
        }

        private BoundStatement BindExpressionStatement(ExpressionStatementSyntax syntax)
        {
            var expression = BindExpression(syntax.Expression, canBeVoid: true);

            return new BoundExpressionStatement(expression);
        }

        private BoundStatement BindReturnStatement(ReturnStatementSyntax syntax)
        {
            if (_function is null)
            {
                _diagnostics.ReportReturnOutsideFunction(syntax.Span);
                return BindErrorStatement();
            }

            var expression = syntax.Value is not null
               ? BindExpression(syntax.Value)
               : null;

            if (_function.Type == TypeSymbol.Void)
            {
                if (expression.Type is not null)
                {
                    _diagnostics.ReportInvalidReturnStatement(syntax.Value.Span, isVoidFunction: true);
                    return BindErrorStatement();
                }

                return new BoundReturnStatement(expression);
            }

            if (expression.Type is null)
            {
                _diagnostics.ReportInvalidReturnStatement(syntax.Span, isVoidFunction: false);
                return BindErrorStatement();
            }

            expression = BindTypeCastExpression(syntax.Value, _function.Type, allowExplicit: false);

            return new BoundReturnStatement(expression);
        }

        private static BoundStatement BindErrorStatement()
        {
            return new BoundExpressionStatement(new BoundErrorExpression());
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
                SyntaxKind.IndexingExpression => BindIndexingExpression(syntax as IndexingExpressionSyntax),
                SyntaxKind.ListExpresion => BindListExpression(syntax as ListExpressionSyntax),
                _ => throw new Exception($"Unexpected syntax {syntax.Kind}"),
            };
        }

        // does not access instance data
        private static BoundExpression BindLiteralExpression(LiteralExpressionSyntax syntax)
        {
            var value = syntax.Value ?? 0;
            return new BoundLiteralExpression(value);
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

                return BindTypeCastExpression(syntax.Arguments[0], t, allowExplicit: true);
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

        private BoundExpression BindTypeCastExpression(ExpressionSyntax syntax, TypeSymbol type, bool allowExplicit = false)
        {
            var expression = BindExpression(syntax);

            var conversion = TypeCasting.Classify(expression.Type, type);

            if (!conversion.Exists)
            {
                if (expression.Type != TypeSymbol.Error && type != TypeSymbol.Error)
                    _diagnostics.ReportCannotConvertTypes(syntax.Span, expression.Type, type);

                return new BoundErrorExpression();
            }

            if (!allowExplicit && conversion.IsExplicit)
            {
                _diagnostics.ReportCannotConvertTypesImplicitly(syntax.Span, expression.Type, type);
            }

            if (conversion.IsIdentity)
                return expression;

            return new BoundTypeCastExpression(type, expression);
        }

        private BoundExpression BindIndexingExpression(IndexingExpressionSyntax syntax)
        {
            if (!_scope.TryLookupVariable(syntax.Identifier.Text, out var list))
            {
                _diagnostics.ReportUndefinedName(syntax.Identifier.Span, syntax.Identifier.Text);
                return new BoundErrorExpression();
            }

            var index = BindExpression(syntax.Index, TypeSymbol.Int);

            return new BoundIndexingExpression(list, index);
        }

        private BoundExpression BindListExpression(ListExpressionSyntax syntax)
        {
            var builder = ImmutableArray.CreateBuilder<BoundExpression>();

            if (!syntax.Initializer.Any())
                return new BoundListExpression(ImmutableArray<BoundExpression>.Empty);

            var first = BindExpression(syntax.Initializer[0]);
            var type = first.Type;
            builder.Add(first);

            for (int i = 1; i < syntax.Initializer.Count; i++)
            {
                var expression = BindExpression(syntax.Initializer[i], type);
                builder.Add(expression);
            }

            return new BoundListExpression(builder.ToImmutable());
        }

        private VariableSymbol BindVariable(SyntaxToken identifier, bool isConst, TypeSymbol type)
        {
            var name = identifier.Text ?? "<error>";
            var shouldDeclare = !identifier.IsMissing;

            var variable = _function is null
                                    ? new GlobalVariableSymbol(name, isConst, type) as VariableSymbol
                                    : new LocalVariableSymbol(name, isConst, type);

            if (shouldDeclare && !_scope.TryDeclareVariable(variable))
                _diagnostics.ReportSymbolAlreadyDeclared(identifier.Span, identifier.Text);
            return variable;
        }
    }
}