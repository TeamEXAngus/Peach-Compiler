using Peach.CodeAnalysis.Syntax;
using System.Collections.Generic;
using Xunit;

namespace Peach_Tests.CodeAnalysis.Syntax
{
    public class ParserTests
    {
        [Theory]
        [MemberData(nameof(GetBinaryOperatorPairsData))]
        public void Parser_BinaryExpression_HonorsPrecedence(SyntaxKind op1, SyntaxKind op2)
        {
            var op1Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op1);
            var op2Precedence = SyntaxFacts.GetBinaryOperatorPrecedence(op2);
            var op1Text = SyntaxFacts.GetText(op1);
            var op2Text = SyntaxFacts.GetText(op2);
            var text = $"a {op1Text} b {op2Text} c";
            var expression = SyntaxTree.Parse(text).Root;

            if (op1Precedence >= op2Precedence)
            {
                //       op2
                //      /   \
                //    op1    c
                //   /   \
                //  a     b

                using var e = new AssertingEnumerator(expression);
                e.AssertNode(SyntaxKind.CompilationUnit);           //  CompilationUnit
                e.AssertNode(SyntaxKind.BinaryExpression);          //  ├──BinaryExpression
                e.AssertNode(SyntaxKind.BinaryExpression);          //  │  ├──BinaryExpression
                e.AssertNode(SyntaxKind.NameExpression);            //  │  │   ├──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "a");     //  │  │   │   └──IdentifierToken a
                e.AssertToken(op1, op1Text);                        //  │  │   ├──<op1>
                e.AssertNode(SyntaxKind.NameExpression);            //  │  │   └──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "b");     //  │  │       └──IdentifierToken b
                e.AssertToken(op2, op2Text);                        //  │  ├──<op2>
                e.AssertNode(SyntaxKind.NameExpression);            //  │  └──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "c");     //  │      └──IdentifierToken c
                e.AssertToken(SyntaxKind.EOFToken, string.Empty);   //  └──EOFToken
            }
            else
            {
                //    op1
                //   /   \
                //  a    op2
                //      /   \
                //     b     c

                using var e = new AssertingEnumerator(expression);
                e.AssertNode(SyntaxKind.CompilationUnit);           //  CompilationUnit
                e.AssertNode(SyntaxKind.BinaryExpression);          //  ├──BinaryExpression
                e.AssertNode(SyntaxKind.NameExpression);            //  │  ├──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "a");     //  │  │   └──IdentifierToken a
                e.AssertToken(op1, op1Text);                        //  │  ├──<op1>
                e.AssertNode(SyntaxKind.BinaryExpression);          //  │  └──BinaryExpression
                e.AssertNode(SyntaxKind.NameExpression);            //  │      ├──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "b");     //  │      │   └──IdentifierToken b
                e.AssertToken(op2, op2Text);                        //  │      ├──<op2>
                e.AssertNode(SyntaxKind.NameExpression);            //  │      └──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "c");     //  │          └──IdentifierToken c
                e.AssertToken(SyntaxKind.EOFToken, string.Empty);   //  └──EOFToken
            }
        }

        [Theory]
        [MemberData(nameof(GetUnaryOperatorPairsData))]
        public void Parser_UnaryExpression_HonorsPrecedence(SyntaxKind unaryKind, SyntaxKind binaryKind)
        {
            var unaryPrecedence = SyntaxFacts.GetUnaryOperatorPrecedence(unaryKind);
            var binaryPrecedence = SyntaxFacts.GetBinaryOperatorPrecedence(binaryKind);
            var unaryText = SyntaxFacts.GetText(unaryKind);
            var binaryText = SyntaxFacts.GetText(binaryKind);
            var text = $"{unaryText} a {binaryText} b";
            var expression = SyntaxTree.Parse(text).Root;

            if (unaryPrecedence >= binaryPrecedence)
            {
                //   binary
                //   /   \
                // unary  b
                //  |
                //  a

                using var e = new AssertingEnumerator(expression);
                e.AssertNode(SyntaxKind.CompilationUnit);           //  CompilationUnit
                e.AssertNode(SyntaxKind.BinaryExpression);          //  ├──BinaryExpression
                e.AssertNode(SyntaxKind.UnaryExpression);           //  │  ├──UnaryExpression
                e.AssertToken(unaryKind, unaryText);                //  │  │   ├──<op1>
                e.AssertNode(SyntaxKind.NameExpression);            //  │  │   └──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "a");     //  │  │       └──IdentifierToken a
                e.AssertToken(binaryKind, binaryText);              //  │  ├──<op2>
                e.AssertNode(SyntaxKind.NameExpression);            //  │  └──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "b");     //  │      └──IdentifierToken b
                e.AssertToken(SyntaxKind.EOFToken, string.Empty);   //  └──EOFToken
            }
            else
            {
                //   unary
                //     |
                //   binary
                //   /   \
                //  a     b

                using var e = new AssertingEnumerator(expression);
                e.AssertNode(SyntaxKind.CompilationUnit);           //  CompilationUnit
                e.AssertNode(SyntaxKind.UnaryExpression);           //  ├──UnaryExpression
                e.AssertToken(unaryKind, unaryText);                //  │  ├──<op1>
                e.AssertNode(SyntaxKind.BinaryExpression);          //  │  └──BinaryExpression
                e.AssertNode(SyntaxKind.NameExpression);            //  │      ├──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "a");     //  │      │   └──IdentifierToken a
                e.AssertToken(binaryKind, binaryText);              //  │      ├──<op2>
                e.AssertNode(SyntaxKind.NameExpression);            //  │      └──NameExpression
                e.AssertToken(SyntaxKind.IdentifierToken, "b");     //  │          └──IdentifierToken b
                e.AssertToken(SyntaxKind.EOFToken, string.Empty);   //  └──EOFToken
            }
        }

        public static IEnumerable<object[]> GetBinaryOperatorPairsData()
        {
            foreach (var op1 in SyntaxFacts.GetBinaryOperatorKinds())
            {
                foreach (var op2 in SyntaxFacts.GetBinaryOperatorKinds())
                {
                    yield return new object[] { op1, op2 };
                }
            }
        }

        public static IEnumerable<object[]> GetUnaryOperatorPairsData()
        {
            foreach (var unary in SyntaxFacts.GetUnaryOperatorKinds())
            {
                foreach (var binary in SyntaxFacts.GetBinaryOperatorKinds())
                {
                    yield return new object[] { unary, binary };
                }
            }
        }
    }
}