using Peach.CodeAnalysis.Symbols;
using System;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundFunctionCallExpression : BoundExpression
    {
        public BoundFunctionCallExpression(FunctionSymbol function, ImmutableArray<BoundExpression> arguments)
        {
            Function = function;
            Arguments = arguments;
        }

        public override TypeSymbol Type => Function.Type;
        public override BoundNodeKind Kind => BoundNodeKind.FunctionCallExpression;
        public FunctionSymbol Function { get; }
        public ImmutableArray<BoundExpression> Arguments { get; }
    }
}