using Peach.CodeAnalysis.Symbols;
using System.Collections.Immutable;
using System.Linq;

namespace Peach.CodeAnalysis.Binding
{
    internal class BoundListExpression : BoundExpression
    {
        public BoundListExpression(ImmutableArray<BoundExpression> contents)
        {
            Contents = contents;
            var type = Contents.ElementAtOrDefault(0)?.Type;
            Type = ListTypeSymbol.GetOrGenerateListTypeSymbol(type ?? TypeSymbol.Int);
        }

        public override BoundNodeKind Kind => BoundNodeKind.ListExpression;
        public ImmutableArray<BoundExpression> Contents { get; }
        public override TypeSymbol Type { get; }
    }
}