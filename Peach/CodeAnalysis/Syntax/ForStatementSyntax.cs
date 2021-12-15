using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class ForStatementSyntax : StatementSyntax
    {
        public ForStatementSyntax(SyntaxToken forKeyword, SyntaxToken variable, SyntaxToken fromKeyword, ExpressionSyntax start,
                                  SyntaxToken toKeyword, ExpressionSyntax stop, SyntaxToken stepKeyword, ExpressionSyntax step,
                                  StatementSyntax body)
        {
            ForKeyword = forKeyword;
            Variable = variable;
            FromKeyword = fromKeyword;
            Start = start;
            ToKeyword = toKeyword;
            Stop = stop;
            StepKeyword = stepKeyword;
            Step = step;
            Body = body;
        }

        public override SyntaxKind Kind => SyntaxKind.ForStatement;
        public SyntaxToken ForKeyword { get; }
        public SyntaxToken Variable { get; }
        public SyntaxToken FromKeyword { get; }
        public ExpressionSyntax Start { get; }
        public SyntaxToken ToKeyword { get; }
        public ExpressionSyntax Stop { get; }
        public SyntaxToken StepKeyword { get; }
        public ExpressionSyntax Step { get; }
        public StatementSyntax Body { get; }

        public override IEnumerable<SyntaxNode> GetChildren()
        {
            yield return ForKeyword;
            yield return Variable;
            yield return FromKeyword;
            yield return Start;
            yield return ToKeyword;
            yield return Stop;
            yield return StepKeyword;
            yield return Step;
            yield return Body;
        }
    }
}