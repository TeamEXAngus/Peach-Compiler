using System.Collections.Generic;

namespace Peach.CodeAnalysis.Symbols
{
    public class ListTypeSymbol : TypeSymbol
    {
        private static Dictionary<TypeSymbol, ListTypeSymbol> _listTypes;

        private TypeSymbol ContainedType { get; }

        private ListTypeSymbol(string name, TypeSymbol type)
            : base(name, type.TypeID)
        {
            ContainedType = type;
        }

        public TypeSymbol GetContainedType()
        {
            return ContainedType;
        }

        public static ListTypeSymbol GetOrGenerateListTypeSymbol(TypeSymbol type)
        {
            if (_listTypes is null)
                _listTypes = new();
            else if (_listTypes.ContainsKey(type))
                return _listTypes[type];

            var symbol = new ListTypeSymbol($"[{type.Name}]", type);
            _listTypes.Add(type, symbol);

            return symbol;

        }
    }
}