using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundForStatement : BoundLoop
    {
        public BoundForStatement(VariableSymbol variable, BoundExpression start, BoundExpression stop, BoundExpression step, BoundStatement body, BoundLabel breakLabel, BoundLabel continueLabel)
            : base(breakLabel, continueLabel)
        {
            Variable = variable;
            Start = start;
            Stop = stop;
            Step = step;
            Body = body;
        }

        public override BoundNodeKind Kind => BoundNodeKind.ForStatement;
        public VariableSymbol Variable { get; }
        public BoundExpression Start { get; }
        public BoundExpression Stop { get; }
        public BoundExpression Step { get; }
        public BoundStatement Body { get; }
    }
}