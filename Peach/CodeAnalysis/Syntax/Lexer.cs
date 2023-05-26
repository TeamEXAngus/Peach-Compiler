using Peach.CodeAnalysis.Symbols;
using Peach.CodeAnalysis.Text;
using System.Text;

namespace Peach.CodeAnalysis.Syntax
{
    internal sealed class Lexer
    {
        private readonly SourceText _text;
        private readonly DiagnosticBag _diagnostics = new();

        private int _position;

        private int _start;
        private SyntaxKind _kind;
        private object _value;

        public DiagnosticBag Diagnostics => _diagnostics;

        public Lexer(SourceText text)
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

        public SyntaxToken Lex()
        {
            _start = _position;
            _kind = SyntaxKind.BadToken;
            _value = null;

            switch (Current)
            {
                case '\0':
                    _kind = SyntaxKind.EOFToken;
                    break;

                case '+':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.PlusEqualsToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.PlusToken;
                        _position++;
                    }
                    break;

                case '-':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.MinusEqualsToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.MinusToken;
                        _position++;
                    }
                    break;

                case '*':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.AsteriskEqualsToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.AsteriskToken;
                        _position++;
                    }
                    break;

                case '/':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.SlashEqualsToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.SlashToken;
                        _position++;
                    }
                    break;

                case '%':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.PercentEqualsToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.PercentToken;
                        _position++;
                    }
                    break;

                case '(':
                    _kind = SyntaxKind.OpenParenToken;
                    _position++;
                    break;

                case ')':
                    _kind = SyntaxKind.CloseParenToken;
                    _position++;
                    break;

                case '{':
                    _kind = SyntaxKind.OpenBraceToken;
                    _position++;
                    break;

                case '}':
                    _kind = SyntaxKind.CloseBraceToken;
                    _position++;
                    break;

                case '[':
                    _kind = SyntaxKind.OpenBracketToken;
                    _position++;
                    break;

                case ']':
                    _kind = SyntaxKind.CloseBracketToken;
                    _position++;
                    break;

                case '~':
                    _kind = SyntaxKind.TildeToken;
                    _position++;
                    break;

                case '^':
                    _kind = SyntaxKind.CaretToken;
                    _position++;
                    break;

                case '&':
                    if (Lookahead == '&')
                    {
                        _kind = SyntaxKind.AmpersandAmpersandToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.AmpersandToken;
                        _position += 1;
                    }
                    break;

                case '|':
                    if (Lookahead == '|')
                    {
                        _kind = SyntaxKind.PipePipeToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.PipeToken;
                        _position += 1;
                    }
                    break;

                case '!':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.ExclamationEqualsToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.ExclamationToken;
                        _position += 1;
                    }
                    break;

                case '=':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.EqualsEqualsToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.EqualsToken;
                        _position += 1;
                    }
                    break;

                case '<':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.LessOrEqualToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.LessThanToken;
                        _position += 1;
                    }
                    break;

                case '>':
                    if (Lookahead == '=')
                    {
                        _kind = SyntaxKind.GreaterOrEqualToken;
                        _position += 2;
                    }
                    else
                    {
                        _kind = SyntaxKind.GreaterThanToken;
                        _position += 1;
                    }
                    break;

                case ',':
                    _kind = SyntaxKind.CommaToken;
                    _position++;
                    break;

                case ':':
                    _kind = SyntaxKind.ColonToken;
                    _position++;
                    break;

                case '"':
                    ReadStringToken();
                    break;

                case '0' or '1' or '2' or '3' or '4':
                case '5' or '6' or '7' or '8' or '9':
                    ReadNumberToken();
                    break;

                case ' ' or '\t' or '\n' or '\r':   // common whitespace
                    ReadWhitespaceToken();
                    break;

                default:
                    if (char.IsLetter(Current))
                    {
                        ReadIdentifierOrKeyword();
                    }
                    else if (char.IsWhiteSpace(Current))    // handle uncommon whitespace
                    {
                        ReadWhitespaceToken();
                    }
                    else
                    {
                        _diagnostics.ReportBadCharacter(_position, Current);
                        _position++;
                    }
                    break;
            }

            var length = _position - _start;
            var text = SyntaxFacts.GetText(_kind);
            if (text is null)
                text = _text.ToString(_start, length);

            return new SyntaxToken(_kind, _start, text, _value);
        }

        private void ReadStringToken()
        {
            _position++;
            var sb = new StringBuilder();
            var done = false;

            while (!done)
            {
                switch (Current)
                {
                    case '\0' or '\r' or '\n':
                        var span = new TextSpan(_start, 1);
                        _diagnostics.ReportUnterminatedString(span);
                        done = true;
                        break;

                    case '\\':
                        if (Lookahead == '"')
                        {
                            sb.Append('"');
                            _position += 2;
                        }
                        else
                        {
                            sb.Append(Current);
                            _position++;
                        }
                        break;

                    case '"':
                        done = true;
                        _position++;
                        break;

                    default:
                        sb.Append(Current);
                        _position++;
                        break;
                }
            }

            _kind = SyntaxKind.StringToken;
            _value = sb.ToString();
        }

        private void ReadNumberToken()
        {
            while (char.IsDigit(Current))
                _position++;

            var len = _position - _start;
            var text = _text.ToString(_start, len);
            if (!int.TryParse(text, out var value))
                _diagnostics.ReportInvalidNumber(new TextSpan(_start, len), text, TypeSymbol.Int);

            _value = value;
            _kind = SyntaxKind.NumberToken;
        }

        private void ReadWhitespaceToken()
        {
            while (char.IsWhiteSpace(Current))
                _position++;

            _kind = SyntaxKind.WhitespaceToken;
        }

        private void ReadIdentifierOrKeyword()
        {
            while (char.IsLetter(Current))
                _position++;

            var length = _position - _start;
            var text = _text.ToString(_start, length);
            _kind = SyntaxFacts.GetKeywordKind(text);
        }
    }
}