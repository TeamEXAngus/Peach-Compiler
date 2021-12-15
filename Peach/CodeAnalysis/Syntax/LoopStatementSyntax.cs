using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class LoopStatementSyntax : StatementSyntax
    {
        public LoopStatementSyntax(SyntaxToken loopKeyword, StatementSyntax body)
        {
            LoopKeyword = loopKeyword;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.LoopStatement;
        public SyntaxToken LoopKeyword { get; }
        public StatementSyntax Body { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return LoopKeyword;
            yield return Body;
        }
    }
}