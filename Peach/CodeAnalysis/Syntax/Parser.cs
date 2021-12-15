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
            var expression = ParseAssignmentExpression();
            var endOfFileToken = MatchToken(SyntaxKind.EOFToken);

            return new CompilationUnitSyntax(expression, endOfFileToken);
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
                SyntaxKind.IdentifierToken or _ => ParseNameExpression(),
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

        private ExpressionSyntax ParseNameExpression()
        {
            var identifierToken = MatchToken(SyntaxKind.IdentifierToken);
            return new NameExpressionSyntax(identifierToken);
        }
    }
}