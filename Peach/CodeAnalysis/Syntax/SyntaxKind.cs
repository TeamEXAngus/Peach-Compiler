using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peach.CodeAnalysis.Syntax
{
    public enum SyntaxKind
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
        ExclamationToken,
        AmpersandAmpersandToken,
        AmpersandToken,
        PipePipeToken,
        PipeToken,
        EqualsEqualsToken,
        ExclamationEqualsToken,
        EqualsToken,
        OpenParenToken,
        CloseParenToken,
        IdentifierToken,

        // Keywords

        TrueKeyword,
        FalseKeyword,

        // Expressions

        LiteralExpression,
        BinaryExpression,
        UnaryExpression,
        ParenthesisedExpression,
    }
}