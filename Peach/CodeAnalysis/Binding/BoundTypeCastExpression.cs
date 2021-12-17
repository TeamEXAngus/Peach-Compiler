using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis.Binding
{
    internal class BoundTypeCastExpression : BoundExpression
    {
        public override TypeSymbol Type { get; }
        public BoundExpression Expression { get; }

        public override BoundNodeKind Kind => BoundNodeKind.TypeCastExpression;

        public BoundTypeCastExpression(TypeSymbol type, BoundExpression expression)
        {
            Type = type;
            Expression = expression;
        }
    }
}