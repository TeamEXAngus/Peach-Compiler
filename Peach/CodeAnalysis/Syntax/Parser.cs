using Peach.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Syntax
{
    internal sealed class Parser
    {
        private readonly ImmutableArray<SyntaxToken> _tokens;
        private int _position;
        private readonly DiagnosticBag _diagnostics = new();
        private readonly SourceText _text;

        private SyntaxToken previous;

        public DiagnosticBag Diagnostics => _diagnostics;

        public Parser(SourceText text)
        {
            _text = text;
            var tokens = new List<SyntaxToken>();
            var lexer = new Lexer(text);
            SyntaxToken token;
            do
            {
                token = lexer.Lex();

                if (token.Kind != SyntaxKind.WhitespaceToken &&
                    token.Kind != SyntaxKind.BadToken)
                {
                    tokens.Add(token);
                }
            } while (token.Kind != SyntaxKind.EOFToken);

            _tokens = tokens.ToImmutableArray();
            _diagnostics.AddRange(lexer.Diagnostics);
        }

        private SyntaxToken Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _tokens.Length)
                return _tokens[^1];

            return _tokens[index];
        }

        private SyntaxToken Current => Peek(0);

        private SyntaxToken NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private SyntaxToken MatchToken(SyntaxKind kind)
        {
            PreventInfiniteLoop();

            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var members = ParseMembers();
            var endOfFileToken = MatchToken(SyntaxKind.EOFToken);

            return new CompilationUnitSyntax(members, endOfFileToken);
        }

        private ImmutableArray<MemberSyntax> ParseMembers()
        {
            var members = ImmutableArray.CreateBuilder<MemberSyntax>();

            while (Current.Kind != SyntaxKind.EOFToken)
            {
                var startToken = Current;

                var member = ParseMember();
                members.Add(member);

                if (Current == startToken) // If no token consumed
                    NextToken();           // skip so no infinite loop
            }

            return members.ToImmutable();
        }

        private MemberSyntax ParseMember()
        {
            if (Current.Kind == SyntaxKind.DefKeyword)
                return ParseFunctionDeclaration();

            return ParseGlobalStatement();
        }

        private MemberSyntax ParseFunctionDeclaration()
        {
            var defKeyword = MatchToken(SyntaxKind.DefKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParen = MatchToken(SyntaxKind.OpenParenToken);
            var parameters = ParseParameterList();
            var closeParen = MatchToken(SyntaxKind.CloseParenToken);
            var typeClause = ParseTypeClause(Optional: true);
            var body = ParseBlockStatement();

            return new FunctionDeclarationSyntax(defKeyword, identifier, openParen, parameters, closeParen, typeClause, body);
        }

        private SeparatedSyntaxList<ParameterSyntax> ParseParameterList()
        {
            var builder = ImmutableArray.CreateBuilder<SyntaxNode>();

            while (Current.Kind != SyntaxKind.CloseParenToken &&
                   Current.Kind != SyntaxKind.EOFToken)
            {
                var identifier = MatchToken(SyntaxKind.IdentifierToken);
                var typeClause = ParseTypeClause();
                var parameter = new ParameterSyntax(identifier, typeClause);
                builder.Add(parameter);

                if (Current.Kind != SyntaxKind.CloseParenToken) // e.g. this isn't the last argument
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    builder.Add(comma);
                }
            }

            return new SeparatedSyntaxList<ParameterSyntax>(builder.ToImmutable());
        }

        private MemberSyntax ParseGlobalStatement()
        {
            var statement = ParseStatement();
            return new GlobalStatementSyntax(statement);
        }

        private StatementSyntax ParseStatement()
        {
            return Current.Kind switch
            {
                SyntaxKind.OpenBraceToken => ParseBlockStatement(),
                SyntaxKind.LetKeyword or SyntaxKind.ConstKeyword => ParseVariableDeclaration(),
                SyntaxKind.IfKeyword => ParseIfStatement(),
                SyntaxKind.WhileKeyword => ParseWhileStatement(),
                SyntaxKind.LoopKeyword => ParseLoopStatement(),
                SyntaxKind.ForKeyword => ParseForStatement(),
                SyntaxKind.ContinueKeyword => ParseContinueStatement(),
                SyntaxKind.BreakKeyword => ParseBreakStatement(),
                _ => ParseExpressionStatement(),
            };
        }

        private BlockStatementSyntax ParseBlockStatement()
        {
            var statements = ImmutableArray.CreateBuilder<StatementSyntax>();

            var openBraceToken = MatchToken(SyntaxKind.OpenBraceToken);

            while (Current.Kind != SyntaxKind.EOFToken &&
                   Current.Kind != SyntaxKind.CloseBraceToken)
            {
                var startToken = Current;

                var statement = ParseStatement();
                statements.Add(statement);

                if (Current == startToken) // If no token consumed
                    NextToken();           // skip so no infinite loop
            }

            var closeBraceToken = MatchToken(SyntaxKind.CloseBraceToken);

            return new BlockStatementSyntax(openBraceToken, statements.ToImmutable(), closeBraceToken);
        }

        private VariableDeclarationSyntax ParseVariableDeclaration()
        {
            var expectedKeyword = Current.Kind == SyntaxKind.ConstKeyword ? SyntaxKind.ConstKeyword : SyntaxKind.LetKeyword;
            var keyword = MatchToken(expectedKeyword);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var typeClause = ParseTypeClause(Optional: true);
            var equals = MatchToken(SyntaxKind.EqualsToken);
            var intializer = ParseAssignmentExpression();
            return new VariableDeclarationSyntax(keyword, identifier, typeClause, equals, intializer);
        }

        private TypeClauseSyntax ParseTypeClause(bool Optional = false)
        {
            if (Optional && Current.Kind != SyntaxKind.ColonToken)
                return null;

            var colonToken = MatchToken(SyntaxKind.ColonToken);

            var type = ParseType();

            return new TypeClauseSyntax(colonToken, type);
        }

        private TypeSyntax ParseType()
        {
            if (Current.Kind == SyntaxKind.OpenBracketToken)
                return ParseListType();

            return ParseTypeName();
        }

        private TypeNameSyntax ParseTypeName()
        {
            var identifier = Current.Kind.IsTypeKeyword() ? NextToken() : MatchToken(SyntaxKind.IdentifierToken);
            return new TypeNameSyntax(identifier);
        }

        private ListTypeSyntax ParseListType()
        {
            var openBracketToken = MatchToken(SyntaxKind.OpenBracketToken);
            var type = ParseType();
            var closeBacketToken = MatchToken(SyntaxKind.CloseBracketToken);

            return new ListTypeSyntax(openBracketToken, type, closeBacketToken);
        }

        private IfStatementSyntax ParseIfStatement()
        {
            var keyword = MatchToken(SyntaxKind.IfKeyword);

            var notKeyword = Current.Kind == SyntaxKind.NotKeyword ? NextToken() : null;

            var condition = ParseParenthesisedExpression();
            var statement = ParseStatement();
            var elseClause = ParseElseClause();

            return new IfStatementSyntax(keyword, notKeyword, condition, statement, elseClause);
        }

        private ElseClauseSyntax ParseElseClause()
        {
            if (Current.Kind != SyntaxKind.ElseKeyword)
                return null;

            var keyword = NextToken();
            var statement = ParseStatement();
            return new ElseClauseSyntax(keyword, statement);
        }

        private WhileStatementSyntax ParseWhileStatement()
        {
            var keyword = MatchToken(SyntaxKind.WhileKeyword);

            var notKeyword = Current.Kind == SyntaxKind.NotKeyword ? NextToken() : null;

            var condition = ParseParenthesisedExpression();
            var statement = ParseStatement();

            return new WhileStatementSyntax(keyword, notKeyword, condition, statement);
        }

        private StatementSyntax ParseLoopStatement()
        {
            var keyword = MatchToken(SyntaxKind.LoopKeyword);
            var body = ParseStatement();

            return new LoopStatementSyntax(keyword, body);
        }

        private ForStatementSyntax ParseForStatement()
        {
            var forKeyword = MatchToken(SyntaxKind.ForKeyword);
            var variable = MatchToken(SyntaxKind.IdentifierToken);
            var fromKeyword = MatchToken(SyntaxKind.FromKeyword);
            var start = ParseExpression();
            var toKeyword = MatchToken(SyntaxKind.ToKeyword);
            var stop = ParseExpression();
            var stepKeyword = MatchToken(SyntaxKind.StepKeyword);
            var step = ParseExpression();
            var body = ParseStatement();

            return new ForStatementSyntax(forKeyword, variable, fromKeyword, start, toKeyword, stop, stepKeyword, step, body);
        }

        private ContinueStatementSyntax ParseContinueStatement()
        {
            var keyword = MatchToken(SyntaxKind.ContinueKeyword);
            return new ContinueStatementSyntax(keyword);
        }

        private BreakStatementSyntax ParseBreakStatement()
        {
            var keyword = MatchToken(SyntaxKind.BreakKeyword);
            return new BreakStatementSyntax(keyword);
        }

        private ExpressionStatementSyntax ParseExpressionStatement()
        {
            var expression = ParseAssignmentExpression();
            return new ExpressionStatementSyntax(expression);
        }

        private ExpressionSyntax ParseAssignmentExpression()
        {
            if (Peek(0).Kind == SyntaxKind.IdentifierToken &&
                Peek(1).Kind == SyntaxKind.EqualsToken)
            {
                var identifierToken = NextToken();
                var operatorToken = NextToken();
                var right = ParseAssignmentExpression();
                return new AssignmentExpressionSyntax(identifierToken, operatorToken, right);
            }

            if (Peek(0).Kind == SyntaxKind.OpenBracketToken)
                return ParseListExpression();

            return ParseExpression();
        }

        private ExpressionSyntax ParseListExpression()
        {
            var openBracketToken = MatchToken(SyntaxKind.OpenBracketToken);

            var builder = ImmutableArray.CreateBuilder<SyntaxNode>();

            while (Current.Kind != SyntaxKind.CloseBracketToken &&
                   Current.Kind != SyntaxKind.EOFToken)
            {
                var expression = ParseAssignmentExpression();
                builder.Add(expression);

                if (Current.Kind != SyntaxKind.CloseBracketToken)
                {
                    var commaToken = MatchToken(SyntaxKind.CommaToken);
                    builder.Add(commaToken);
                }
            }

            var closeBracketToken = MatchToken(SyntaxKind.CloseBracketToken);

            var expressions = new SeparatedSyntaxList<ExpressionSyntax>(builder.ToImmutable());

            return new ListExpressionSyntax(openBracketToken, expressions, closeBracketToken);
        }

        private ExpressionSyntax ParseExpression(int parentPrecedence = 0)
        {
            ExpressionSyntax left;

            var unaryOperatorPrecedence = Current.Kind.GetUnaryOperatorPrecedence();
            if (unaryOperatorPrecedence != 0 && unaryOperatorPrecedence >= parentPrecedence)
            {
                var operatorToken = NextToken();
                var operand = ParseExpression(unaryOperatorPrecedence);
                left = new UnaryExpressionSyntax(operatorToken, operand);
            }
            else
            {
                left = ParsePrimaryExpression();
            }

            for (; ; )
            {
                var precedence = Current.Kind.GetBinaryOperatorPrecedence();
                if (precedence == 0 || precedence <= parentPrecedence)
                {
                    break;
                }

                var operatorToken = NextToken();
                var right = ParseExpression(precedence);
                left = new BinaryExpressionSyntax(left, operatorToken, right);
            }

            return left;
        }

        private void PreventInfiniteLoop()
        {
            if (previous == Current)
                NextToken();

            previous = Current;
        }

        public ExpressionSyntax ParsePrimaryExpression()
        {
            return Current.Kind switch
            {
                SyntaxKind.OpenParenToken => ParseParenthesisedExpression(),
                SyntaxKind.FalseKeyword or SyntaxKind.TrueKeyword => ParseBooleanLiteral(),
                SyntaxKind.NumberToken => ParseNumberLiteral(),
                SyntaxKind.StringToken => ParseStringLiteral(),
                SyntaxKind.IdentifierToken or _ => ParseNameOrCallExpression(),
            };
        }

        private ExpressionSyntax ParseParenthesisedExpression()
        {
            var left = MatchToken(SyntaxKind.OpenParenToken);
            var expression = ParseAssignmentExpression();
            var right = MatchToken(SyntaxKind.CloseParenToken);
            return new ParenthesisedExpressionSyntax(left, expression, right);
        }

        private ExpressionSyntax ParseBooleanLiteral()
        {
            var isTrue = Current.Kind == SyntaxKind.TrueKeyword;
            var KeywordToken = isTrue ? MatchToken(SyntaxKind.TrueKeyword) : MatchToken(SyntaxKind.FalseKeyword);
            return new LiteralExpressionSyntax(KeywordToken, isTrue);
        }

        private ExpressionSyntax ParseNumberLiteral()
        {
            var numberToken = MatchToken(SyntaxKind.NumberToken);
            return new LiteralExpressionSyntax(numberToken);
        }

        private ExpressionSyntax ParseStringLiteral()
        {
            var stringToken = MatchToken(SyntaxKind.StringToken);
            return new LiteralExpressionSyntax(stringToken);
        }

        private ExpressionSyntax ParseNameOrCallExpression()
        {
            if ((Peek(0).Kind == SyntaxKind.IdentifierToken || Peek(0).Kind.IsTypeKeyword()) &&
                 Peek(1).Kind == SyntaxKind.OpenParenToken)
                return ParseFunctionCallExpreson();

            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenBracketToken)
                return ParseIndexingExpression();

            return ParseNameExpression();
        }

        private ExpressionSyntax ParseFunctionCallExpreson()
        {
            var identifier = NextToken();
            var openParenToken = MatchToken(SyntaxKind.OpenParenToken);
            var argList = ParseArgumentList();
            var closeParenToken = MatchToken(SyntaxKind.CloseParenToken);

            return new FunctionCallExpressionSyntax(identifier, openParenToken, argList, closeParenToken);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArgumentList()
        {
            var builder = ImmutableArray.CreateBuilder<SyntaxNode>();

            var parseNext = true;

            while (parseNext &&
                   Current.Kind != SyntaxKind.CloseParenToken &&
                   Current.Kind != SyntaxKind.EOFToken)
            {
                var expression = ParseExpression();
                builder.Add(expression);

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    builder.Add(comma);
                }
                else
                {
                    parseNext = false;
                }
            }

            return new SeparatedSyntaxList<ExpressionSyntax>(builder.ToImmutable());
        }

        private ExpressionSyntax ParseIndexingExpression()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openBracketToken = MatchToken(SyntaxKind.OpenBracketToken);
            var index = ParseAssignmentExpression();
            var closeBracketToken = MatchToken(SyntaxKind.CloseBracketToken);

            return new IndexingExpressionSyntax(identifier, openBracketToken, index, closeBracketToken);
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(identifierToken);
        }
    }
}