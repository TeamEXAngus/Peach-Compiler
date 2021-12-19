namespace Peach.CodeAnalysis.Binding
{
    internal abstract class BoundLoop : BoundStatement
    {
        protected BoundLoop(BoundLabel breakLabel, BoundLabel continueLabel)
        {
            BreakLabel = breakLabel;
            ContinueLabel = continueLabel;
        }

        public BoundLabel BreakLabel { get; }
        public BoundLabel ContinueLabel { get; }
    }
}