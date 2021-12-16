﻿using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundLiteralExpresion : BoundExpression
    {
        public BoundLiteralExpresion(object value)
        {
            Value = value;

            if (value is bool)
                Type = TypeSymbol.Bool;
            else if (value is int)
                Type = TypeSymbol.Int;
            else if (value is string)
                Type = TypeSymbol.String;
            else
                throw null;
        }

        public override BoundNodeKind Kind => BoundNodeKind.LiteralExpression;
        public override TypeSymbol Type { get; }
        public object Value { get; }
    }
}