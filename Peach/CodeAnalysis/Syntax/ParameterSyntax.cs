using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public class ParameterSyntax : SyntaxNode
    {
        public ParameterSyntax(SyntaxToken identifier, TypeClauseSyntax typeClause)
        {
            Identifier = identifier;
            TypeClause = typeClause;
        }

        public override SyntaxKind Kind => SyntaxKind.Paramater;
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return TypeClause;
        }
    }
}