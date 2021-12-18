using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class FunctionCallExpressionSyntax : ExpressionSyntax
    {
        public FunctionCallExpressionSyntax(SyntaxToken identifier, SyntaxToken openParenToken, SeparatedSyntaxList<ExpressionSyntax> arguments, SyntaxToken closeParenToken)
        {
            Identifier = identifier;
            OpenParenToken = openParenToken;
            Arguments = arguments;
            CloseParenToken = closeParenToken;
        }

        public override SyntaxKind Kind => SyntaxKind.FunctionCallExpression;
        public SyntaxToken Identifier { get; }
        public SyntaxToken OpenParenToken { get; }
        public SeparatedSyntaxList<ExpressionSyntax> Arguments { get; }
        public SyntaxToken CloseParenToken { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return Identifier;
            yield return OpenParenToken;
            foreach (var arg in Arguments.NodesAndSeparators)
                yield return arg;

            yield return CloseParenToken;
        }
    }
}