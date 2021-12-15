using System;

namespace Peach.CodeAnalysis.Binding
{
    internal class BoundParenthesisedExpression : BoundExpression
    {
        public BoundParenthesisedExpression(BoundExpression expression)
        {
            Expression = expression;
        }

        public BoundExpression Expression { get; }

        public override Type Type => Expression.Type;

        public override BoundNodeKind Kind => BoundNodeKind.ParenthesisedExpression;
    }
}