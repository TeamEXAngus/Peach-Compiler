using System.Collections.Generic;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(ImmutableArray<MemberSyntax> members, SyntaxToken eofToken)
        {
            Members = members;
            EOFToken = eofToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ImmutableArray<MemberSyntax> Members { get; }
        public SyntaxToken EOFToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            foreach (var s in Members)
                yield return s;
            yield return EOFToken;
        }
    }
}