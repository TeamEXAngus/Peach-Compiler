using System.Collections.Generic;
using System.Linq;

namespace Peach.CodeAnalysis.Symbols
{
    public sealed class TypeSymbol : Symbol
    {
        public static readonly List<TypeSymbol> AllTypes = new();

        public static readonly TypeSymbol Error = new("?", TypeID.Error);
        public static readonly TypeSymbol Void = new("void", TypeID.Void);

        public static readonly TypeSymbol Int = new("int", TypeID.Int);
        public static readonly TypeSymbol Bool = new("bool", TypeID.Bool);
        public static readonly TypeSymbol String = new("string", TypeID.String);

        private TypeSymbol(string name, TypeID typeID)
            : base(name)
        {
            TypeID = typeID;
            AllTypes.Add(this);
        }

        public TypeID TypeID { get; }

        public override SymbolKind Kind => SymbolKind.Type;

        public static TypeSymbol LookupTypeFromText(string text)
        {
            return AllTypes.SingleOrDefault(t => t.Name == text);
        }

        public static TypeSymbol LookupType(TypeID typeID)
        {
            return typeID switch
            {
                TypeID.Error => Error,
                TypeID.Void => Void,

                TypeID.Int => Int,
                TypeID.Bool => Bool,
                TypeID.String => String,

                _ => throw new System.Exception($"Unknown TypeID {typeID}"),
            };
        }

        public override string ToString()
        {
            return Name;
        }
    }
}