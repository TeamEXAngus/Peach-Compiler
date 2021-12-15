namespace Peach.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        UnaryExpression,
        BinaryExpression,
        LiteralExpression,
        ParenthesisedExpression,
        VariableExpression,
        AssignmentExpression
    }
}