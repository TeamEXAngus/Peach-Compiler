using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peach.CodeAnalysis
{
    internal class Evaluator
    {
        private readonly ExpressionSyntax _root;

        public Evaluator(ExpressionSyntax root)
        {
            _root = root;
        }

        public int Evaluate()
        {
            return EvaluateExpression(_root);
        }

        private int EvaluateExpression(ExpressionSyntax root)
        {
            // BinaryExpression
            // NumberExpression

            if (root is NumberExpressionSyntax n)
                return (int)n.NumberToken.Value;

            if (root is BinaryExpressionSyntax b)
            {
                var left = EvaluateExpression(b.Left);
                var right = EvaluateExpression(b.Right);

                if (b.OperatorToken.Kind == SyntaxKind.TokenPlus)
                    return left + right;
                if (b.OperatorToken.Kind == SyntaxKind.TokenMinus)
                    return left - right;
                if (b.OperatorToken.Kind == SyntaxKind.TokenAsterisk)
                    return left * right;
                if (b.OperatorToken.Kind == SyntaxKind.TokenForwardSlash)
                    return left / right;

                throw new Exception($"Unexpected binary operator '{b.OperatorToken.Kind}'");
            }

            if (root is ParenthesisedExpressionSyntax p)
                return EvaluateExpression(p.Expression);

            throw new Exception($"Unexpected node '{root.Kind}'");
        }
    }
}