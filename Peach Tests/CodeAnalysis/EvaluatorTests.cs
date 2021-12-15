﻿using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Syntax;
using System.Collections.Generic;
using Xunit;

namespace Peach_Tests.CodeAnalysis
{
    public class EvaluatorTests
    {
        private static readonly Dictionary<VariableSymbol, object> _variables = new();

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
                var compilation2 = compilation1.ContinueWith(syntaxTree2);
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
            yield return new object[] { "!!true", true };
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
            foreach (var (line1, line2, result) in GetVariableDataWithoutKeyword())
            {
                yield return new object[] { "let " + line1, line2, result };
                yield return new object[] { "const " + line1, line2, result };
            }

            foreach (var d in GetMutableVariableData())
                yield return d;
        }

        private static IEnumerable<(string line1, string line2, object result)> GetVariableDataWithoutKeyword()
        {
            yield return ("a = 1", "a", 1);
            yield return ("a = 1", "-a", -1);
            yield return ("a = 2", "a + a", 4);
            yield return ("a = 2", "a - a", 0);
            yield return ("a = 2", "a * a", 4);
            yield return ("a = 2", "a / a", 1);
            yield return ("a = 1 + 2 == 3", "a", true);
            yield return ("a = 1 + 2 == 3", "!a", false);
        }

        private static IEnumerable<object[]> GetMutableVariableData()
        {
            yield return new object[] { "let a = 10", "a = a - 1", 9 };
            yield return new object[] { "let a = 10", "a = a + 1", 11 };
            yield return new object[] { "let a = 10", "a = a * 2", 20 };
            yield return new object[] { "let a = 10", "a = a / 2", 5 };
            yield return new object[] { "let a = true", "a = !a", false };
            yield return new object[] { "let a = false", "a = !a", true };
            yield return new object[] { "let a = true", "a = a && a", true };
            yield return new object[] { "let a = false", "a = a && a", false };
        }
    }
}