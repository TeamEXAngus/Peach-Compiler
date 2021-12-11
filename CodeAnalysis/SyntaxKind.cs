using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peach.CodeAnalysis
{
    internal enum SyntaxKind
    {
        TokenWhitespace,
        TokenEOF,
        TokenNumber,
        TokenPlus,
        TokenMinus,
        TokenAsterisk,
        TokenForwardSlash,
        TokenOpenParen,
        TokenCloseParen,
        BadToken,

        NumberExpression,
        BinaryExpression,
        ParenthesisedExpression
    }
}