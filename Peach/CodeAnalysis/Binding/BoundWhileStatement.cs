namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement : BoundStatement
    {
        public BoundWhileStatement(BoundExpression condition, bool negated, BoundStatement body)
        {
            Condition = condition;
            Negated = negated;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.WhileStatement;
        public BoundExpression Condition { get; }
        public bool Negated { get; }
        public BoundStatement Body { get; }
    }
}