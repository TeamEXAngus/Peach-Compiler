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
        DefKeyword,
        IntKeyword,
        BoolKeyword,
        StringKeyword,

        // Expressions

        LiteralExpression,
        ListExpresion,
        BinaryExpression,
        UnaryExpression,
        ParenthesisedExpression,
        NameExpression,
        AssignmentExpression,
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