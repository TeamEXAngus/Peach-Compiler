namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement : BoundStatement
    {
        public BoundWhileStatement(BoundExpression condition, bool isNegated, BoundStatement body)
        {
            Condition = condition;
            IsNegated = isNegated;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition { get; }
        public bool IsNegated { get; }
        public BoundStatement Body { get; }
    }
}