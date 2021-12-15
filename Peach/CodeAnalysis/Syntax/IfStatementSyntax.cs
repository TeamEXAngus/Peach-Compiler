using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class IfStatementSyntax : StatementSyntax
    {
        public IfStatementSyntax(SyntaxToken ifKeyword, SyntaxToken notKeyword, ExpressionSyntax condition, StatementSyntax thenStatement, ElseClauseSyntax elseClause)
        {
            IfKeyword = ifKeyword;

            NotKeyword = notKeyword;
            Condition = condition;
            ThenStatement = thenStatement;
            ElseClause = elseClause;
        }

        public override SyntaxKind Kind => SyntaxKind.IfStatement;
        public SyntaxToken IfKeyword { get; }
        public SyntaxToken NotKeyword { get; }
        public ExpressionSyntax Condition { get; }
        public StatementSyntax ThenStatement { get; }
        public ElseClauseSyntax ElseClause { get; }

        public bool IsNegated => NotKeyword is not null;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return IfKeyword;
            if (NotKeyword is not null)
                yield return NotKeyword;
            yield return Condition;
            yield return ThenStatement;
            if (ElseClause is not null)
                yield return ElseClause;
        }
    }
}