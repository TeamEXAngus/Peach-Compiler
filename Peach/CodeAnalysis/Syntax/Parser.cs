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
            if (index > _tokens.Length)
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
            if (Current.Kind == kind)
                return NextToken();

            _diagnostics.ReportUnexpectedToken(Current.Span, Current.Kind, kind);
            return new SyntaxToken(kind, Current.Position, null, null);
        }

        public CompilationUnitSyntax ParseCompilationUnit()
        {
            var statment = ParseStatement();
            var endOfFileToken = MatchToken(SyntaxKind.EOFToken);

            return new CompilationUnitSyntax(statment, endOfFileToken);
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
            var typeClause = ParseTypeClause();
            var equals = MatchToken(SyntaxKind.EqualsToken);
            var intializer = ParseExpression();
            return new VariableDeclarationSyntax(keyword, identifier, typeClause, equals, intializer);
        }

        private TypeClauseSyntax ParseTypeClause()
        {
            if (Current.Kind != SyntaxKind.ColonToken)
                return null;

            var colonToken = MatchToken(SyntaxKind.ColonToken);
            var identifier = MatchToken(SyntaxKind.IdentifierToken);

            return new TypeClauseSyntax(colonToken, identifier);
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

            return ParseExpression();
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
            var expression = ParseExpression();
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
            if (Peek(0).Kind == SyntaxKind.IdentifierToken && Peek(1).Kind == SyntaxKind.OpenParenToken)
                return ParseFunctionCallExpreson();

            return ParseNameExpression();
        }

        private ExpressionSyntax ParseFunctionCallExpreson()
        {
            var identifier = MatchToken(SyntaxKind.IdentifierToken);
            var openParenToken = MatchToken(SyntaxKind.OpenParenToken);
            var argList = ParseArgumentList();
            var closeParenToken = MatchToken(SyntaxKind.CloseParenToken);

            return new FunctionCallExpressionSyntax(identifier, openParenToken, argList, closeParenToken);
        }

        private SeparatedSyntaxList<ExpressionSyntax> ParseArgumentList()
        {
            var builder = ImmutableArray.CreateBuilder<SyntaxNode>();

            while (Current.Kind != SyntaxKind.CloseParenToken &&
                   Current.Kind != SyntaxKind.EOFToken)
            {
                var expression = ParseExpression();
                builder.Add(expression);

                if (Current.Kind != SyntaxKind.CloseParenToken) // e.g. this isn't the last argument
                {
                    var comma = MatchToken(SyntaxKind.CommaToken);
                    builder.Add(comma);
                }
            }

            return new SeparatedSyntaxList<ExpressionSyntax>(builder.ToImmutable());
        }

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(identifierToken);
        }
    }
}