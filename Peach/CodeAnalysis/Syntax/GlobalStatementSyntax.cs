using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class GlobalStatementSyntax : MemberSyntax
    {
        public GlobalStatementSyntax(StatementSyntax statement)
        {
            Statement = statement;
        }

        public StatementSyntax Statement { get; }

        public override SyntaxKind Kind => SyntaxKind.GlobalStatement;

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Statement;
        }
    }
}