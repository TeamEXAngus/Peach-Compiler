namespace Peach.CodeAnalysis.Binding
{
    internal enum BoundBinaryOperatorKind
    {
        Addition,
        Subtraction,
        Multiplication,
        Division,
        LogicalAnd,
        LogicalOr,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        Equality,
        Inequality,
        LessThan,
        LessOrEqual,
        GreaterThan,
        GreaterOrEqual
    }
}