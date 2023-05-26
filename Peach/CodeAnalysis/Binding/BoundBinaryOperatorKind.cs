namespace Peach.CodeAnalysis.Binding
{
    internal enum BoundBinaryOperatorKind
    {
        Addition,
        Subtraction,
        Multiplication,
        StringAddition,
        Division,
        Modulo,
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