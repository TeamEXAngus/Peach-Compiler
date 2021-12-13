using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Peach_Tests.CodeAnalysis
{
    public class EvaluatorTests
    {
        private static Dictionary<VariableSymbol, object> _variables = new();

        [Theory]
        [MemberData(nameof(GetExpressionData))]
        public void Evaluator_EvaluatesExpressionCorrectly(string text, object expectedResult)
        {
            try
            {
                var syntaxTree = SyntaxTree.Parse(text);
                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate(_variables);

                Assert.Empty(result.Diagnostics);
                Assert.Equal(expectedResult, result.Value);
            }
            finally
            {
                _variables.Clear();
            }
        }

        [Theory]
        [MemberData(nameof(GetVariableData))]
        public void Evaluator_EvaluatesVariableCorrectly(string line1, string line2, object expectedResult)
        {
            try
            {
                var syntaxTree1 = SyntaxTree.Parse(line1);
                var compilation1 = new Compilation(syntaxTree1);
                var result1 = compilation1.Evaluate(_variables);

                Assert.Empty(result1.Diagnostics);

                var syntaxTree2 = SyntaxTree.Parse(line2);
                var compilation2 = new Compilation(syntaxTree2);
                var result2 = compilation2.Evaluate(_variables);

                Assert.Empty(result2.Diagnostics);
                Assert.Equal(expectedResult, result2.Value);
            }
            finally
            {
                _variables.Clear();
            }
        }

        public static IEnumerable<object[]> GetExpressionData()
        {
            yield return new object[] { "1", 1 };
            yield return new object[] { "+1", 1 };
            yield return new object[] { "-1", -1 };
            yield return new object[] { "--1", 1 };
            yield return new object[] { "(1)", 1 };
            yield return new object[] { "-(1)", -1 };
            yield return new object[] { "(-1)", -1 };
            yield return new object[] { "-(-1)", 1 };
            yield return new object[] { "1 + 2 + 3", 6 };
            yield return new object[] { "1 - 2 + 3", 2 };
            yield return new object[] { "1 + 2 * 3", 7 };
            yield return new object[] { "(1 + 2) * 3", 9 };
            yield return new object[] { "4 / 2", 2 };
            yield return new object[] { "9 / 3", 3 };

            yield return new object[] { "true", true };
            yield return new object[] { "false", false };
            yield return new object[] { "!true", false };
            yield return new object[] { "!false", true };
            yield return new object[] { "true == true", true };
            yield return new object[] { "false == false", true };
            yield return new object[] { "true != false", true };
            yield return new object[] { "true == false", false };
            yield return new object[] { "true == false", false };
            yield return new object[] { "true != true", false };
            yield return new object[] { "false != false", false };
            yield return new object[] { "true && true", true };
            yield return new object[] { "false && true", false };
            yield return new object[] { "false && false", false };
            yield return new object[] { "true || true", true };
            yield return new object[] { "false || true", true };
            yield return new object[] { "false || false", false };
            yield return new object[] { "(1 + 2 == 3)", true };
            yield return new object[] { "(1 + 2 != 4)", true };
            yield return new object[] { "(1 + 2 != 3)", false };
            yield return new object[] { "(1 + 2 == 4)", false };
        }

        public static IEnumerable<object[]> GetVariableData()
        {
            yield return new object[] { "a = 1", "a", 1 };
            yield return new object[] { "a = 1", "-a", -1 };
            yield return new object[] { "a = b = 2", "a + b", 4 };
            yield return new object[] { "a = b = 2", "a - b", 0 };
            yield return new object[] { "a = b = 2", "a * b", 4 };
            yield return new object[] { "a = b = 2", "a / b", 1 };
            yield return new object[] { "a = 1 + 2 == 3", "a", true };
            yield return new object[] { "a = 1 + 2 == 3", "!a", false };
        }
    }
}