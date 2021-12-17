namespace Peach.CodeAnalysis.Symbols
{
    public abstract class VariableSymbol : Symbol
    {
        internal VariableSymbol(string name, bool isConst, TypeSymbol type)
            : base(name)
        {
            IsConst = isConst;
            Type = type;
        }

        public bool IsConst { get; }
        public TypeSymbol Type { get; }
    }

    public sealed class GlobalVariableSymbol : VariableSymbol
    {
        internal GlobalVariableSymbol(string name, bool isConst, TypeSymbol type)
            : base(name, isConst, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.GlobalVariable;
    }

    public class LocalVariableSymbol : VariableSymbol
    {
        internal LocalVariableSymbol(string name, bool isConst, TypeSymbol type)
            : base(name, isConst, type)
        {
        }

        public override SymbolKind Kind => SymbolKind.LocalVariable;
    }
}