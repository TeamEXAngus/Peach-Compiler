using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class ReturnStatementSyntax : StatementSyntax
    {
        public ReturnStatementSyntax(SyntaxNode keyword, ExpressionSyntax value)
        {
            Keyword = keyword;
            Value = value;
        }

        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

        public SyntaxNode Keyword { get; }
        public ExpressionSyntax Value { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Keyword;
            if (Value is not null)
                yield return Value;
        }
    }
}