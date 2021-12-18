using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class IndexingExpressionSyntax : ExpressionSyntax
    {
        public IndexingExpressionSyntax(SyntaxToken identifier, SyntaxToken openBracketToken, ExpressionSyntax index, SyntaxToken closeBracketToken)
        {
            Identifier = identifier;
            OpenBracketToken = openBracketToken;
            Index = index;
            CloseBracketToken = closeBracketToken;
        }

        public override SyntaxKind Kind => SyntaxKind.IndexingExpression;
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenBracketToken { get; }
        public ExpressionSyntax Index { get; }
        public SyntaxToken CloseBracketToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return OpenBracketToken;
            yield return Index;
            yield return CloseBracketToken;
        }
    }
}