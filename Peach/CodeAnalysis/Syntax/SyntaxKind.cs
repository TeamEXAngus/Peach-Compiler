namespace Peach.CodeAnalysis.Syntax
{
    public enum SyntaxKind
    {
        // Tokens

        BadToken,
        EOFToken,
        WhitespaceToken,
        NumberToken,
        PlusToken,
        MinusToken,
        AsteriskToken,
        SlashToken,
        AmpersandAmpersandToken,
        AmpersandToken,
        PipePipeToken,
        PipeToken,
        EqualsEqualsToken,
        ExclamationToken,
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

        // Keywords

        TrueKeyword,
        FalseKeyword,
        LetKeyword,
        ConstKeyword,

        // Expressions

        LiteralExpression,
        BinaryExpression,
        UnaryExpression,
        ParenthesisedExpression,
        NameExpression,
        AssignmentExpression,

        // Nodes

        CompilationUnit,

        // Statements

        BlockStatement,
        ExpressionStatement,
        VariableDeclaration,
    }
}