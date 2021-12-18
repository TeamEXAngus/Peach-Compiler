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

        public override string ToString()
        {
            return base.ToString() + $"of type {Type}";
        }
    }
}