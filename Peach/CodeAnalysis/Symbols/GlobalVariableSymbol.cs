namespace Peach.CodeAnalysis.Symbols
{
    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        internal GlobalVariableSymbol(string name, bool isConst, TypeSymbol type)
            : base(name, isConst, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.GlobalVariable;
    }
}