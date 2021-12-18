using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class ListExpressionSyntax : ExpressionSyntax
    {
        public ListExpressionSyntax(SyntaxToken openBracketToken, SeparatedSyntaxList<ExpressionSyntax> initializer, SyntaxToken closeBracketToken)
        {
            OpenBracketToken = openBracketToken;
            Initializer = initializer;
            CloseBracketToken = closeBracketToken;
        }

        public override SyntaxKind Kind => SyntaxKind.ListExpresion;

        public SyntaxToken OpenBracketToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Initializer { get; }
        public SyntaxToken CloseBracketToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBracketToken;

            foreach (var expression in Initializer.NodesAndSeparators)
                yield return expression;

            yield return CloseBracketToken;
        }
    }
}