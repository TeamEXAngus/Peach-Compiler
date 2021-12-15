using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(StatementSyntax statement, SyntaxToken eofToken)
        {
            Statement = statement;
            EOFToken = eofToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public StatementSyntax Statement { get; }
        public SyntaxToken EOFToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Statement;
            yield return EOFToken;
        }
    }
}