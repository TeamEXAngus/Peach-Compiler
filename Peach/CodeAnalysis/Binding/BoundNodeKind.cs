namespace Peach.CodeAnalysis.Binding
{
    internal enum BoundNodeKind
    {
        // Statements

        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,

        // Expressions

        BinaryExpression,
        LiteralExpression,
        ParenthesisedExpression,
        VariableExpression,
        AssignmentExpression,
    }
}