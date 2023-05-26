using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class CompoundAssignmentExpressionSyntax : ExpressionSyntax
    {
        public CompoundAssignmentExpressionSyntax(SyntaxToken identifierToken, SyntaxToken operatorToken, ExpressionSyntax expression)
        {
            IdentifierToken = identifierToken;
            OperatorToken = operatorToken;
            Expression = expression;
        }

        public override SyntaxKind Kind => SyntaxKind.CompoundAssignmentExpression;
        public SyntaxToken IdentifierToken { get; }
        public SyntaxToken OperatorToken { get; }
        public ExpressionSyntax Expression { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IdentifierToken;
            yield return OperatorToken;
            yield return Expression;
        }
    }
}