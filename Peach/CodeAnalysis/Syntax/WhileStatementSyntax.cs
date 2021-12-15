using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class WhileStatementSyntax : StatementSyntax
    {
        public WhileStatementSyntax(SyntaxToken whileKeyword, SyntaxToken notKeyword, ExpressionSyntax condition, StatementSyntax body)

        {
            WhileKeyword = whileKeyword;
            NotKeyword = notKeyword;
            Condition = condition;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.WhileStatement;
        public SyntaxToken WhileKeyword { get; }
        public SyntaxToken NotKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax Body { get; }

        public bool IsNegated => NotKeyword is not null;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return WhileKeyword;
            if (NotKeyword is not null)
                yield return NotKeyword;
            yield return Condition;
            yield return Body;
        }
    }
}