using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis.Binding
{
    internal class BoundParenthesisedExpression : BoundExpression
    {
        public BoundParenthesisedExpression(BoundExpression expression)
        {
            Expression = expression;
        }

        public BoundExpression Expression { get; }

        public override TypeSymbol Type => Expression.Type;

        public override BoundNodeKind Kind => BoundNodeKind.ParenthesisedExpression;
    }
}