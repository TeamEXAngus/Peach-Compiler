namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement(BoundExpression condition, bool negated, BoundStatement thenStatement, BoundStatement elseStatement)
        {
            Condition = condition;
            Negated = negated;
            ThenStatment = thenStatement;
            ElseStatement = elseStatement;
        }

        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;
        public BoundExpression Condition { get; }
        public bool Negated { get; }
        public BoundStatement ThenStatment { get; }
        public BoundStatement ElseStatement { get; }
    }
}