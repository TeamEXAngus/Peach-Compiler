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

                _ => 0
            };
        }

        internal static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => 1,
                SyntaxKind.MinusToken => 1,
                SyntaxKind.AsteriskToken => 2,
                SyntaxKind.SlashToken => 2,

                _ => 0
            };
        }
    }
}