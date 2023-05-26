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
        PlusEqualsToken,
        MinusToken,
        MinusEqualsToken,
        AsteriskToken,
        AsteriskEqualsToken,
        SlashToken,
        SlashEqualsToken,
        PercentToken,
        PercentEqualsToken,
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
        OpenBracketToken,
        CloseBracketToken,
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
        ContinueKeyword,
        BreakKeyword,
        DefKeyword,
        ReturnKeyword,
        IntKeyword,
        BoolKeyword,
        StringKeyword,
        EntrypointKeyword,

        // Expressions

        LiteralExpression,
        BinaryExpression,
        UnaryExpression,
        ParenthesisedExpression,
        NameExpression,
        AssignmentExpression,
        CompoundAssignmentExpression,
        FunctionCallExpression,
        IndexingExpression,

        // Nodes

        CompilationUnit,
        GlobalStatement,
        FunctionDeclaration,
        ElseClause,
        TypeClause,
        TypeName,
        Paramater,

        // Statements

        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
        IfStatement,
        WhileStatement,
        ForStatement,
        LoopStatement,
        ContinueStatement,
        BreakStatement,
        ReturnStatement,
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