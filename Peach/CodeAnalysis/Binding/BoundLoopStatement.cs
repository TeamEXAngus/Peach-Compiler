namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundLoopStatement : BoundLoop
    {
        public BoundLoopStatement(BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
            : base(breakLabel, continueLabel)
        {
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LoopStatement;
        public BoundStatement Body { get; }
    }
}