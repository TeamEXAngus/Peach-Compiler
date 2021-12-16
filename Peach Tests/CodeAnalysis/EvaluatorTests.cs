using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Syntax;
using System;
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

        [Fact]
        public void Evaluator_BlockStatement_NoInfiniteLoop()
        {
            var text = @"
                {
                    [)][]
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.CloseParenToken, SyntaxKind.CloseBraceToken)}
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.CloseParenToken, SyntaxKind.EOFToken)}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ReportUndefinedUnaryOperator()
        {
            var text = @"
                {
                    [-]false
                    [!]10
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportUndefinedUnaryOperatorMessage("-", typeof(bool))}
                {DiagnosticBag.ReportUndefinedUnaryOperatorMessage("!", typeof(int))}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ReportUndefinedBinaryOperator()
        {
            var text = @"
                {
                    true [+] true
                    10 [&&] 10
                    false [*] 7
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportUndefinedBinaryOperatorMessage("+", typeof(bool), typeof(bool))}
                {DiagnosticBag.ReportUndefinedBinaryOperatorMessage("&&", typeof(int), typeof(int))}
                {DiagnosticBag.ReportUndefinedBinaryOperatorMessage("*", typeof(bool), typeof(int))}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ReportUndefinedName()
        {
            var text = @"
                {
                    [peach]
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportUndefinedNameMessage("peach")}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_NoCrashWhenInsertedToken()
        {
            var text = @"[]";

            var diagnostics = @$"
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.IdentifierToken, SyntaxKind.EOFToken)}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ReportVariableAlreadyDeclared()
        {
            var text = @"
                {
                    let x = 10
                    {
                        let x = false
                    }
                    let [x] = 5
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportVariableAlreadyDeclaredMessage("x")}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ReportCannotConvertTypes()
        {
            var text = @"
                {
                    let x = true
                    x = [10]

                    let foo = 20
                    foo = [false]
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportCannotConvertTypesMessage(typeof(bool), typeof(int))}
                {DiagnosticBag.ReportCannotConvertTypesMessage(typeof(int), typeof(bool))}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_IfStatement_ReportCannotConvertTypes()
        {
            var text = @"
                {
                    let x = 0
                    if [(69)]
                    {
                        x = 10
                    }
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportCannotConvertTypesMessage(typeof(bool), typeof(int))}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_WhileStatement_ReportCannotConvertTypes()
        {
            var text = @"
                {
                    let x = 0
                    while [(69)]
                    {
                        x = 10
                    }
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportCannotConvertTypesMessage(typeof(bool), typeof(int))}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ConditionalStatement_ConditionRequiresParen()
        {
            var text = @"
                {
                    if [true] [{]}
                    if (true) {}

                    if not [false] [{]}
                    if not (false) {}

                    while not [true] [{]}
                    while not (true) {}
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.TrueKeyword, SyntaxKind.OpenParenToken)}
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.OpenBraceToken, SyntaxKind.CloseBraceToken)}
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.FalseKeyword, SyntaxKind.OpenParenToken)}
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.OpenBraceToken, SyntaxKind.CloseBraceToken)}
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.TrueKeyword, SyntaxKind.OpenParenToken)}
                {DiagnosticBag.ReportUnexpectedTokenMessage(SyntaxKind.OpenBraceToken, SyntaxKind.CloseBraceToken)}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ForStatement_ReportCannotConvertTypes()
        {
            var text = @"
                {
                    for i from [true] to [false] step [true]
                    { }
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportCannotConvertTypesMessage(typeof(int), typeof(bool))}
                {DiagnosticBag.ReportCannotConvertTypesMessage(typeof(int), typeof(bool))}
                {DiagnosticBag.ReportCannotConvertTypesMessage(typeof(int), typeof(bool))}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ReportCannotAssignToConst()
        {
            var text = @"
                {
                    const x = true
                    x [=] false
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportCannotAssignToConstMessage("x")}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        [Fact]
        public void Evaluator_ForStatement_ReportCannotAssignToConst()
        {
            var text = @"
                {
                    for i from 1 to 10 step 1
                    {
                        i [=] 7
                    }
                }
            ";

            var diagnostics = @$"
                {DiagnosticBag.ReportCannotAssignToConstMessage("i")}
            ";

            AssertHasDiagnostics(text, diagnostics);
        }

        private static void AssertHasDiagnostics(string text, string diagnosticText)
        {
            var annotatedText = AnnotatedText.Parse(text);

            var syntaxTree = SyntaxTree.Parse(annotatedText.Text);
            var compilation = new Compilation(syntaxTree);
            var result = compilation.Evaluate(new Dictionary<VariableSymbol, object>());

            var diagnostics = result.Diagnostics;
            var expectedDiagnostics = AnnotatedText.UnindentLines(diagnosticText);

            if (annotatedText.Spans.Length != expectedDiagnostics.Length)
                throw new Exception("Must have same number of expected diagnostics and annotated spans");

            Assert.Equal(expectedDiagnostics.Length, diagnostics.Length);

            for (var i = 0; i < diagnostics.Length; i++)
            {
                var expectedSpan = annotatedText.Spans[i];
                var actualSpan = diagnostics[i].Span;

                Assert.Equal(expectedSpan, actualSpan);
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
            yield return new object[] { "~1", -2 };
            yield return new object[] { "~~1", 1 };
            yield return new object[] { "-(-1)", 1 };
            yield return new object[] { "1 + 2 + 3", 6 };
            yield return new object[] { "1 - 2 + 3", 2 };
            yield return new object[] { "1 + 2 * 3", 7 };
            yield return new object[] { "(1 + 2) * 3", 9 };
            yield return new object[] { "4 / 2", 2 };
            yield return new object[] { "9 / 3", 3 };

            yield return new object[] { "9 > 3", true };
            yield return new object[] { "9 >= 3", true };
            yield return new object[] { "9 < 3", false };
            yield return new object[] { "9 <= 3", false };
            yield return new object[] { "3 > 9", false };
            yield return new object[] { "3 >= 9", false };
            yield return new object[] { "3 < 9", true };
            yield return new object[] { "3 <= 9", true };
            yield return new object[] { "9 > 9", false };
            yield return new object[] { "9 >= 9", true };
            yield return new object[] { "9 < 9", false };
            yield return new object[] { "9 <= 9", true };

            yield return new object[] { "1 | 2", 3 };
            yield return new object[] { "1 | 0", 1 };
            yield return new object[] { "1 & 3", 1 };
            yield return new object[] { "1 & 0", 0 };
            yield return new object[] { "1 ^ 0", 1 };
            yield return new object[] { "5 ^ 5", 0 };

            yield return new object[] { "true & true", true };
            yield return new object[] { "false & true", false };
            yield return new object[] { "false & false", false };
            yield return new object[] { "true | true", true };
            yield return new object[] { "false | true", true };
            yield return new object[] { "false | false", false };
            yield return new object[] { "true ^ false", true };
            yield return new object[] { "false ^ false", false };
            yield return new object[] { "true ^ true", false };

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

            yield return new object[] { "if (true) {10} else {5}", 10 };
            yield return new object[] { "if (false) {10} else {5}", 5 };
            yield return new object[] { "if not (true) {10} else {5}", 5 };
            yield return new object[] { "if not (false) {10} else {5}", 10 };

            yield return new object[] { "{ let a = 10 if (a == 10) {a = 5} a } ", 5 };
            yield return new object[] { "{ let a = 12 if (a == 10) {a = 5} a } ", 12 };

            yield return new object[] { "{ let a = 0 while (a < 10) {a = a + 1} a }", 10 };
            yield return new object[] { "{ let a = 0 while not (a == 10) {a = a + 1} a }", 10 };
            yield return new object[] { "{ let a = 10 while (a > 0) {a = a - 1} a }", 0 };
            yield return new object[] { "{ let a = 10 while not (a == 0) {a = a - 1} a }", 0 };

            yield return new object[] { "{ let a = 0 for i from 1 to 10 step 1 { a = a + i} a }", 45 };
            yield return new object[] { "{ let a = 0 for i from 0 to 10 step 2 { a = a + i} a }", 20 };
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