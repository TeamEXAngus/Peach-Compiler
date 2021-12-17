using System.Collections.Immutable;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Peach.CodeAnalysis.Symbols
{
    internal static class BuiltinFunctions
    {
        public static readonly FunctionSymbol Print = new("print", ImmutableArray.Create(new ParamaterSymbol("text", TypeSymbol.String)), TypeSymbol.Void);
        public static readonly FunctionSymbol Input = new("input", ImmutableArray<ParamaterSymbol>.Empty, TypeSymbol.String);
        public static readonly FunctionSymbol Rand = new("rand", ImmutableArray<ParamaterSymbol>.Empty, TypeSymbol.Int);
        public static readonly FunctionSymbol RandRange = new("randRange", ImmutableArray.Create(new ParamaterSymbol("lower", TypeSymbol.Int), new ParamaterSymbol("upper", TypeSymbol.Int)), TypeSymbol.Int);

        internal static IEnumerable<FunctionSymbol> GetAll()
            => typeof(BuiltinFunctions).GetFields(BindingFlags.Public | BindingFlags.Static)
                                       .Where(f => f.FieldType == typeof(FunctionSymbol))
                                       .Select(f => f.GetValue(null) as FunctionSymbol);
    }
}