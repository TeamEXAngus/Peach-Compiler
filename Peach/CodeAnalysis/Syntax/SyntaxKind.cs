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
        ColonToken,

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
        IntKeyword,
        BoolKeyword,
        StringKeyword,

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
        GlobalStatement,
        FunctionDeclaration,
        ElseClause,
        TypeClause,
        Paramater,

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