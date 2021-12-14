using System;

namespace Peach.CodeAnalysis
{
    public sealed class VariableSymbol
    {
        internal VariableSymbol(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public Type Type { get; }
    }
}