using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class BreakStatementSyntax : StatementSyntax
    {
        public BreakStatementSyntax(SyntaxToken keyword)
        {
            Keyword = keyword;
        }

        public SyntaxToken Keyword { get; }

        public override SyntaxKind Kind => SyntaxKind.BreakStatement;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }
    }
}