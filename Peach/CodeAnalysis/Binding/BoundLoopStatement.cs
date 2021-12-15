namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundLoopStatement : BoundStatement
    {
        public BoundLoopStatement(BoundStatement body)
        {
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LoopStatement;
        public BoundStatement Body { get; }
    }
}