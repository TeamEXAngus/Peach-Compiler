namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundIfStatement : BoundStatement
    {
        public BoundIfStatement(BoundExpression condition, bool isNegated, BoundStatement thenStatement, BoundStatement elseStatement)
        {
            Condition = condition;
            IsNegated = isNegated;
            ThenStatment = thenStatement;
            ElseStatement = elseStatement;
        }

        public override BoundNodeKind Kind => BoundNodeKind.IfStatement;
        public BoundExpression Condition { get; }
        public bool IsNegated { get; }
        public BoundStatement ThenStatment { get; }
        public BoundStatement ElseStatement { get; }
    }
}