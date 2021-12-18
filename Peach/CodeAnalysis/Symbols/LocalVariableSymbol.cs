namespace Peach.CodeAnalysis.Symbols
{
    public class LocalVariableSymbol : VariableSymbol
    {
        internal LocalVariableSymbol(string name, bool isConst, TypeSymbol type)
            : base(name, isConst, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.LocalVariable;
    }
}