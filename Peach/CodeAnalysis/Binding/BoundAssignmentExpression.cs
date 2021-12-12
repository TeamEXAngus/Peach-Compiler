using System;

namespace Peach.CodeAnalysis.Binding
{
    internal class BoundAssignmentExpression : BoundExpression
    {
        public string Name => Variable.Name;
        public BoundExpression Expression { get; }
        public VariableSymbol Variable { get; }

        public BoundAssignmentExpression(VariableSymbol variable, BoundExpression boundExpression)
        {
            Variable = variable;
            Expression = boundExpression;
        }

        public override Type Type => Expression.Type;

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    }
}