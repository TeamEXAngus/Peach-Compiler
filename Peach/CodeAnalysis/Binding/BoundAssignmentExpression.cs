using System;

namespace Peach.CodeAnalysis.Binding
{
    internal class BoundAssignmentExpression : BoundExpression
    {
        public string Name { get; }
        public BoundExpression Expression { get; }

        public BoundAssignmentExpression(string name, BoundExpression boundExpression)
        {
            Name = name;
            Expression = boundExpression;
        }

        public override Type Type => Expression.Type;

        public override BoundNodeKind Kind => BoundNodeKind.AssignmentExpression;
    }
}