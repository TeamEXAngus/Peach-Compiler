using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly string _text;
        private int _position;
        private DiagnosticBag _diagnostics = new();

        public DiagnosticBag Diagnostics => _diagnostics;

        public Lexer(string text)
        {
            _text = text;
        }

        private char Current
        {
            get
            {
                if (_position >= _text.Length)
                    return '\0';

                return _text[_position];
            }
        }

        private char Lookahead => Peek(1);

        private char Peek(int offset)
        {
            var index = _position + offset;
            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }

        private void Next()
        {
            _position++;
        }

        public SyntaxToken Lex()
        {
            if (_position >= _text.Length)
                return new SyntaxToken(SyntaxKind.EOFToken, _position, "\0", null);

            var start = _position;

            if (char.IsDigit(Current))
            {
                while (char.IsDigit(Current))
                {
                    Next();
                }

                var len = _position - start;
                var text = _text.Substring(start, len);
                if (!int.TryParse(text, out var value))
                {
                    _diagnostics.ReportInvalidNumber(new TextSpan(start, len), _text, typeof(int));
                }
                return new SyntaxToken(SyntaxKind.NumberToken, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                var len = _position - start;
                var text = _text.Substring(start, len);
                return new SyntaxToken(SyntaxKind.WhitespaceToken, start, text, null);
            }

            if (char.IsLetter(Current))
            {
                while (char.IsLetter(Current))
                {
                    Next();
                }

                var length = _position - start;
                var text = _text.Substring(start, length);
                var kind = SyntaxFacts.GetKeywordKind(text);
                return new SyntaxToken(kind, start, text, null);
            }

            switch (Current)
            {
                case '+':
                    return new SyntaxToken(SyntaxKind.PlusToken, _position++, "+", null);

                case '-':
                    return new SyntaxToken(SyntaxKind.MinusToken, _position++, "-", null);

                case '*':
                    return new SyntaxToken(SyntaxKind.AsteriskToken, _position++, "*", null);

                case '/':
                    return new SyntaxToken(SyntaxKind.SlashToken, _position++, "/", null);

                case '(':
                    return new SyntaxToken(SyntaxKind.OpenParenToken, _position++, "(", null);

                case ')':
                    return new SyntaxToken(SyntaxKind.CloseParenToken, _position++, ")", null);

                case '!':
                    if (Lookahead == '=')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxKind.ExclamationEqualsToken, start, "!=", null);
                    }
                    return new SyntaxToken(SyntaxKind.ExclamationToken, _position++, "!", null);

                case '&':
                    if (Lookahead == '&')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxKind.AmpersandAmpersandToken, start, "&&", null);
                    }
                    return new SyntaxToken(SyntaxKind.AmpersandToken, _position++, "&", null);

                case '|':
                    if (Lookahead == '|')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxKind.PipePipeToken, start, "&&", null);
                    }
                    return new SyntaxToken(SyntaxKind.PipeToken, _position++, "&", null);

                case '=':
                    if (Lookahead == '=')
                    {
                        _position += 2;
                        return new SyntaxToken(SyntaxKind.EqualsEqualsToken, start, "==", null);
                    }
                    return new SyntaxToken(SyntaxKind.EqualsToken, _position++, "=", null);
            }

            _diagnostics.ReportBadCharacter(_position, Current);
            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
        }
    }
}