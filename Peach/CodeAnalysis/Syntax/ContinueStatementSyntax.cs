using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class ContinueStatementSyntax : StatementSyntax
    {
        public ContinueStatementSyntax(SyntaxToken keyword)
        {
            Keyword = keyword;
        }

        public SyntaxToken Keyword { get; }

        public override SyntaxKind Kind => SyntaxKind.ContinueStatement;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
        }
    }
}