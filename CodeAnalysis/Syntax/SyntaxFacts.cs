using System;

namespace Peach.CodeAnalysis.Syntax
{
    internal static class SyntaxFacts
    {
        internal static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => 5,
                SyntaxKind.MinusToken => 5,
                SyntaxKind.ExclamationToken => 5,

                _ => 0
            };
        }

        internal static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PipePipeToken => 1,

                SyntaxKind.AmpersandAmpersandToken => 2,

                SyntaxKind.PlusToken => 3,
                SyntaxKind.MinusToken => 3,

                SyntaxKind.AsteriskToken => 4,
                SyntaxKind.SlashToken => 4,

                _ => 0
            };
        }

        internal static SyntaxKind GetKeywordKind(string text)
        {
            return text switch
            {
                "true" => SyntaxKind.TrueKeyword,
                "false" => SyntaxKind.FalseKeyword,
                _ => SyntaxKind.IdentifierToken
            };
        }
    }
}