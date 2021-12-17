using System.Collections.Generic;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(ImmutableArray<MemberSyntax> statements, SyntaxToken eofToken)
        {
            Statements = statements;
            EOFToken = eofToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<MemberSyntax> Statements { get; }
        public SyntaxToken EOFToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var s in Statements)
                yield return s;
            yield return EOFToken;
        }
    }
}