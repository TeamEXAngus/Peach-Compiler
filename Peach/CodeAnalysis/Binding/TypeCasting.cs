using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class TypeCasting
    {
        public static readonly TypeCasting None = new(exists: false, isIdentity: false, isImplicit: false);
        public static readonly TypeCasting Identity = new(exists: true, isIdentity: true, isImplicit: true);
        public static readonly TypeCasting Implicit = new(exists: true, isIdentity: false, isImplicit: true);
        public static readonly TypeCasting Explicit = new(exists: true, isIdentity: false, isImplicit: false);

        private TypeCasting(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        public bool Exists { get; }
        public bool IsIdentity { get; }
        public bool IsImplicit { get; }
        public bool IsExplicit => Exists && !IsImplicit;

        public static TypeCasting Classify(TypeSymbol from, TypeSymbol to)
        {
            if (from == to)
                return Identity;

            return (from.TypeID, to.TypeID) switch
            {
                (TypeID.Bool, TypeID.String) => Explicit,
                (TypeID.Int, TypeID.String) => Explicit,
                (TypeID.String, TypeID.Int) => Explicit,
                (TypeID.String, TypeID.Bool) => Explicit,
                (_, _) => None
            };
        }
    }
}