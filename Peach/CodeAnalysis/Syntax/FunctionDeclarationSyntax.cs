using System.Collections.Generic;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class FunctionDeclarationSyntax : MemberSyntax
    {
        public FunctionDeclarationSyntax(SyntaxToken functionKeyword, ImmutableArray<SyntaxToken> modifiers, SyntaxToken identifier, SyntaxToken openParenToken, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenToken, TypeClauseSyntax typeClause, BlockStatementSyntax body)
        {
            FunctionKeyword = functionKeyword;
            Modifiers = modifiers;
            Identifier = identifier;
            OpenParenToken = openParenToken;
            Parameters = parameters;
            CloseParenToken = closeParenToken;
            TypeClause = typeClause;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionDeclaration;

        public SyntaxToken FunctionKeyword { get; }
        public ImmutableArray<SyntaxToken> Modifiers { get; }
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenToken { get; }
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }
        public SyntaxToken CloseParenToken { get; }
        public TypeClauseSyntax TypeClause { get; }
        public BlockStatementSyntax Body { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return FunctionKeyword;
            foreach (var modifier in Modifiers)
                yield return modifier;
            yield return Identifier;
            yield return OpenParenToken;
            foreach (var token in Parameters.NodesAndSeparators)
                yield return token;
            yield return CloseParenToken;
            if (TypeClause is not null)
                yield return TypeClause;
            yield return Body;
        }
    }
}