namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundWhileStatement : BoundLoop
    {
        public BoundWhileStatement(BoundExpression condition, bool isNegated, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
            : base(breakLabel, continueLabel)
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