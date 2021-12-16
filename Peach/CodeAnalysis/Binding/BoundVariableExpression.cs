using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundVariableExpression : BoundExpression
    {
        public BoundVariableExpression(VariableSymbol variable)
        {
            Variable = variable;
        }

        public override BoundNodeKind Kind => BoundNodeKind.VariableExpression;
        public string Name => Variable.Name;
        public override TypeSymbol Type => Variable.Type;
        public VariableSymbol Variable { get; }
    }
}