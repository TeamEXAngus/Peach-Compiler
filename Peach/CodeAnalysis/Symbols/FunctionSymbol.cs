using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, ImmutableArray<ParamaterSymbol> parameters, TypeSymbol type)
            : base(name)
        {
            Parameters = parameters;
            Type = type;
        }

        public override SymbolKind Kind => SymbolKind.Function;
        public ImmutableArray<ParamaterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
    }
}