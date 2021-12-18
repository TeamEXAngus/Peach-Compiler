using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class TypeClauseSyntax : SyntaxNode
    {
        public TypeClauseSyntax(SyntaxToken colonToken, TypeSyntax type)
        {
            ColonToken = colonToken;
            Type = type;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeClause;
        public SyntaxToken ColonToken { get; }
        public TypeSyntax Type { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ColonToken;

            yield return Type;
        }
    }

    public abstract class TypeSyntax : SyntaxNode
    {
    }

    public sealed class TypeNameSyntax : TypeSyntax
    {
        public TypeNameSyntax(SyntaxToken identifier)
        {
            Identifier = identifier;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeName;

        public SyntaxToken Identifier { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
        }
    }

    public sealed class ListTypeSyntax : TypeSyntax
    {
        public ListTypeSyntax(SyntaxToken openBracketToken, TypeSyntax type, SyntaxToken closeBracketToken)
        {
            OpenBracketToken = openBracketToken;
            Type = type;
            CloseBracketToken = closeBracketToken;
        }

        public override SyntaxKind Kind => SyntaxKind.TypeName;

        public SyntaxToken OpenBracketToken { get; }
        public TypeSyntax Type { get; }
        public SyntaxToken CloseBracketToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return OpenBracketToken;
            yield return Type;
            yield return CloseBracketToken;
        }
    }
}