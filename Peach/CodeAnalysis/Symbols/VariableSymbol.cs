namespace Peach.CodeAnalysis.Symbols
{
    public sealed class VariableSymbol : Symbol
    {
        internal VariableSymbol(string name, bool isConst, TypeSymbol type)
            : base(name)
        {
            IsConst = isConst;
            Type = type;
        }

        public override SymbolKind Kind => SymbolKind.Variable;
        public bool IsConst { get; }
        public TypeSymbol Type { get; }
    }
}