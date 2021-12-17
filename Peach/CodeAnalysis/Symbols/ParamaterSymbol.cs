namespace Peach.CodeAnalysis.Symbols
{
    public sealed class ParamaterSymbol : VariableSymbol
    {
        public ParamaterSymbol(string name, TypeSymbol type)
            : base(name, isConst: true, type)
        { }

        public override SymbolKind Kind => SymbolKind.Parameter;
    }
}