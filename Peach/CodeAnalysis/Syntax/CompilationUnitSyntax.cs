using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        public CompilationUnitSyntax(ExpressionSyntax expression, SyntaxToken eofToken)
        {
            Expression = expression;
            EOFToken = eofToken;
        }

        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;
        public ExpressionSyntax Expression { get; }
        public SyntaxToken EOFToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Expression;
            yield return EOFToken;
        }
    }
}