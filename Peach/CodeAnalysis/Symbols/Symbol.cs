﻿namespace Peach.CodeAnalysis.Symbols
{
    public abstract class Symbol
    {
        protected Symbol(string name)
        {
            Name = name;
        }

        public abstract SymbolKind Kind { get; }
        public string Name { get; }
    }
}