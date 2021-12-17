namespace Peach.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens

        BadToken,
        EOFToken,
        WhitespaceToken,
        NumberToken,
        StringToken,
        PlusToken,
        MinusToken,
        AsteriskToken,
        SlashToken,
        AmpersandToken,
        PipeToken,
        ExclamationToken,
        TildeToken,
        CaretToken,
        AmpersandAmpersandToken,
        PipePipeToken,
        EqualsEqualsToken,
        ExclamationEqualsToken,
        EqualsToken,
        LessOrEqualToken,
        LessThanToken,
        GreaterOrEqualToken,
        GreaterThanToken,
        OpenParenToken,
        CloseParenToken,
        OpenBraceToken,
        CloseBraceToken,
        IdentifierToken,
        CommaToken,

        // Keywords

        TrueKeyword,
        FalseKeyword,
        LetKeyword,
        ConstKeyword,
        IfKeyword,
        ElseKeyword,
        WhileKeyword,
        NotKeyword,
        LoopKeyword,
        ForKeyword,
        FromKeyword,
        ToKeyword,
        StepKeyword,

        // Expressions

        LiteralExpression,
        BinaryExpression,
        UnaryExpression,
        ParenthesisedExpression,
        NameExpression,
        AssignmentExpression,
        FunctionCallExpression,

        // Nodes

        CompilationUnit,
        ElseClause,

        // Statements

        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement,
        LoopStatement,
    }

    public enum TokenKind
    {
        Keyword,
        Operator,
        Literal,
        Identifier,

        None
    }
}