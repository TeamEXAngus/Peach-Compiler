using System;

namespace Peach.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        internal static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => 6,
                SyntaxKind.MinusToken => 6,
                SyntaxKind.ExclamationToken => 6,

                _ => 0
            };
        }

        internal static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PipePipeToken => 1,

                SyntaxKind.AmpersandAmpersandToken => 2,

                SyntaxKind.EqualsEqualsToken => 3,
                SyntaxKind.ExclamationEqualsToken => 3,

                SyntaxKind.PlusToken => 4,
                SyntaxKind.MinusToken => 4,

                SyntaxKind.AsteriskToken => 5,
                SyntaxKind.SlashToken => 5,

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

        public static string GetText(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => "+",
                SyntaxKind.MinusToken => "-",
                SyntaxKind.AsteriskToken => "*",
                SyntaxKind.SlashToken => "/",
                SyntaxKind.AmpersandToken => "&",
                SyntaxKind.AmpersandAmpersandToken => "&&",
                SyntaxKind.PipeToken => "|",
                SyntaxKind.PipePipeToken => "||",
                SyntaxKind.ExclamationToken => "!",
                SyntaxKind.EqualsToken => "=",
                SyntaxKind.EqualsEqualsToken => "==",
                SyntaxKind.ExclamationEqualsToken => "!=",
                SyntaxKind.OpenParenToken => "(",
                SyntaxKind.CloseParenToken => ")",
                SyntaxKind.TrueKeyword => "true",
                SyntaxKind.FalseKeyword => "false",
                _ => null,
            };
        }
    }
}