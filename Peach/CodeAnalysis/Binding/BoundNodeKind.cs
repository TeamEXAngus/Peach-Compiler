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
        LoopStatement,
        ForStatement,

        // Expressions

        BinaryExpression,
        LiteralExpression,
        ParenthesisedExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
    }
}