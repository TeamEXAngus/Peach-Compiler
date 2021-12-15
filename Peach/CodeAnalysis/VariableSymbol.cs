using System;

namespace Peach.CodeAnalysis
{
    public sealed class VariableSymbol
    {
        internal VariableSymbol(string name, bool isConst, Type type)
        {
            Name = name;
            IsConst = isConst;
            Type = type;
        }

        public string Name { get; }
        public bool IsConst { get; }
        public Type Type { get; }
    }
}