using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Binding
{
    internal abstract class BoundStatement : BoundNode
    {
    }

    internal sealed class BoundBlockStatement : BoundStatement
    {
        public BoundBlockStatement(ImmutableArray<BoundStatement> statements)
        {
            Statements = statements;
        }

        public ImmutableArray<BoundStatement> Statements { get; }

        public override BoundNodeKind Kind => BoundNodeKind.BlockStatement;
    }

    internal sealed class BoundExpressionStatement : BoundStatement
    {
        public BoundExpressionStatement(BoundExpression expression)
        {
            Expression = expression;
        }

        public BoundExpression Expression { get; }
        public override BoundNodeKind Kind => BoundNodeKind.ExpressionStatement;
    }
}