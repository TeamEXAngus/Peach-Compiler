using System.Collections.Generic;

namespace Peach.CodeAnalysis
{
    internal class Lexer
    {
        private readonly string _text;
        private int _position;
        private List<string> _diagnostics = new();

        public IEnumerable<string> Diagnostics => _diagnostics;

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

        private void Next()
        {
            _position++;
        }

        public SyntaxToken NextToken()
        {
            if (_position >= _text.Length)
                return new SyntaxToken(SyntaxKind.TokenEOF, _position, "\0", null);

            if (char.IsDigit(Current))
            {
                var start = _position;

                while (char.IsDigit(Current))
                {
                    Next();
                }

                var len = _position - start;
                var text = _text.Substring(start, len);
                if (!int.TryParse(text, out var value))
                {
                    _diagnostics.Add($"ERROR: invalid number '{text}'");
                }
                return new SyntaxToken(SyntaxKind.TokenNumber, start, text, value);
            }

            if (char.IsWhiteSpace(Current))
            {
                var start = _position;

                while (char.IsWhiteSpace(Current))
                {
                    Next();
                }

                var len = _position - start;
                var text = _text.Substring(start, len);
                return new SyntaxToken(SyntaxKind.TokenWhitespace, start, text, null);
            }

            if (Current == '+')

                return new SyntaxToken(SyntaxKind.TokenPlus, _position++, "+", null);

            if (Current == '-')

                return new SyntaxToken(SyntaxKind.TokenMinus, _position++, "-", null);

            if (Current == '*')

                return new SyntaxToken(SyntaxKind.TokenAsterisk, _position++, "*", null);

            if (Current == '/')

                return new SyntaxToken(SyntaxKind.TokenForwardSlash, _position++, "/", null);

            if (Current == '(')

                return new SyntaxToken(SyntaxKind.TokenOpenParen, _position++, "(", null);

            if (Current == ')')

                return new SyntaxToken(SyntaxKind.TokenCloseParen, _position++, ")", null);

            _diagnostics.Add($"ERROR: bad character input: '{Current}'");
            return new SyntaxToken(SyntaxKind.BadToken, _position++, _text.Substring(_position - 1, 1), null);
        }
    }
}