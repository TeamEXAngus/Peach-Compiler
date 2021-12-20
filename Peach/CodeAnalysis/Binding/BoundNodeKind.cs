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
        GotoStatement,
        ConditionalGotoStatement,
        LabelStatement,
        ReturnStatement,

        // Expressions

        BinaryExpression,
        LiteralExpression,
        ParenthesisedExpression,
        VariableExpression,
        AssignmentExpression,
        UnaryExpression,
        FunctionCallExpression,
        TypeCastExpression,
        IndexingExpression,
        ListExpression,
        ErrorExpression,
    }
}