using Peach.CodeAnalysis.Symbols;

namespace Peach.CodeAnalysis.Binding
{
    internal sealed class BoundIndexingExpression : BoundExpression
    {
        public BoundIndexingExpression(VariableSymbol list, BoundExpression index)
        {
            List = list;
            Index = index;
            Type = (List.Type as ListTypeSymbol).GetContainedType();
        }

        public override TypeSymbol Type { get; }
        public override BoundNodeKind Kind => BoundNodeKind.IndexingExpression;
        public VariableSymbol List { get; }
        public BoundExpression Index { get; }
    }
}