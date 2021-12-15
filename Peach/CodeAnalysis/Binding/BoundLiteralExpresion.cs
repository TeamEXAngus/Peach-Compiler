using System;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpresion : BoundExpression
    {
        public BoundLiteralExpresion(object value)
        {
            Value = value;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override Type Type => Value.GetType();
        public object Value { get; }
    }
}