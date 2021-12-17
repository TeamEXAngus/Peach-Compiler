﻿using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class VariableDeclarationSyntax : StatementSyntax
    {
        public VariableDeclarationSyntax(SyntaxToken keyword, SyntaxToken identifier, TypeClauseSyntax typeClause, SyntaxToken equalsToken, ExpressionSyntax initializer)
        {
            Keyword = keyword;
            Identifier = identifier;
            TypeClause = typeClause;
            EqualsToken = equalsToken;
            Initializer = initializer;
        }

        public override SyntaxKind Kind => SyntaxKind.VariableDeclaration;

        public SyntaxToken Keyword { get; }
        public SyntaxToken Identifier { get; }
        public TypeClauseSyntax TypeClause { get; }
        public SyntaxToken EqualsToken { get; }
        public ExpressionSyntax Initializer { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            yield return Identifier;
            if (TypeClause is not null)
                yield return TypeClause;
            yield return EqualsToken;
            yield return Initializer;
        }
    }
}