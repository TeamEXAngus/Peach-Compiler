using System.Collections.Immutable;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Peach.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol Print = new("print", ImmutableArray.Create(new ParameterSymbol("text", TypeSymbol.String)), TypeSymbol.Void);
        public static readonly FunctionSymbol Input = new("input", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.String);
        public static readonly FunctionSymbol Rand = new("rand", ImmutableArray<ParameterSymbol>.Empty, TypeSymbol.Int);
        public static readonly FunctionSymbol RandRange = new("randRange", ImmutableArray.Create(new ParameterSymbol("lower", TypeSymbol.Int), new ParameterSymbol("upper", TypeSymbol.Int)), TypeSymbol.Int);

        internal static IEnumerable<FunctionSymbol> GetAll()
            => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                       .Where(f => f.FieldType == typeof(FunctionSymbol))
                                       .Select(f => f.GetValue(null) as FunctionSymbol);
    }
}