using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peach.CodeAnalysis.Syntax
{
    internal enum SyntaxKind
    {
        // Tokens
        BadToken,

        EOFToken,
        WhitespaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        AsteriskToken,
        SlashToken,
        OpenParenToken,
        CloseParenToken,

        // Expressions
        LiteralExpression,

        BinaryExpression,
        UnaryExpression,
        ParenthesisedExpression
    }
}