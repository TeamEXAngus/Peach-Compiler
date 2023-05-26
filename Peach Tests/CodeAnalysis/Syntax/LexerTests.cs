using Peach.CodeAnalysis.Syntax;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using System;
using Peach.CodeAnalysis;

namespace Peach_Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        [Fact]
        public void Lexer_Tests_AllTokens()
        {
            var tokenKinds = Enum.GetValues(typeof(SyntaxKind))
                                .Cast<SyntaxKind>()
                                .Where(k => k.ToString().EndsWith("Keyword") ||
                                            k.ToString().EndsWith("Token"));

            var testedTokenKinds = GetTokens()
                .Concat(GetSeparators())
                .Select(t => t.kind);

            var untestedTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);
            untestedTokenKinds.Remove(SyntaxKind.EOFToken);
            untestedTokenKinds.Remove(SyntaxKind.BadToken);
            untestedTokenKinds.ExceptWith(testedTokenKinds);

            Assert.Empty(untestedTokenKinds);
        }

        [Fact]
        public void Lexer_Error_UnterminatedString()
        {
            var text = @"
            {
                let foo = ""foo""
                let bar = [""]bar
            }
            ";

            var diagnostics = $@"
                {DiagnosticBag.GetUnterminatedStringErrorMesage()}
            ";

            AssertingEnumerator.AssertHasDiagnostics(text, diagnostics);
        }

        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(SyntaxKind kind, string text)
        {
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

        /*  Deprecated
            Compound assignment operators cause problems

        [Theory]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(SyntaxKind t1kind, string t1text,
                                          SyntaxKind t2kind, string t2text)
        {
            var tokens = SyntaxTree.ParseTokens(t1text + t2text).ToArray();

            Assert.Equal(2, tokens.Length);

            Assert.Equal(tokens[0].Kind, t1kind);
            Assert.Equal(tokens[0].Text, t1text);

            Assert.Equal(tokens[1].Kind, t2kind);
            Assert.Equal(tokens[1].Text, t2text);
        }
        */

        [Theory]
        [MemberData(nameof(GetTokenPairsWithSeparatorData))]
        public void Lexer_Lexes_TokenPairsWithSeparator(SyntaxKind t1kind, string t1text,
                                                        SyntaxKind sepKind, string sepText,
                                                        SyntaxKind t2kind, string t2text)
        {
            var tokens = SyntaxTree.ParseTokens(t1text + sepText + t2text).ToArray();

            Assert.Equal(3, tokens.Length);

            Assert.Equal(tokens[0].Kind, t1kind);
            Assert.Equal(tokens[0].Text, t1text);

            Assert.Equal(tokens[1].Kind, sepKind);
            Assert.Equal(tokens[1].Text, sepText);

            Assert.Equal(tokens[2].Kind, t2kind);
            Assert.Equal(tokens[2].Text, t2text);
        }

        public static IEnumerable<object[]> GetTokensData()
        {
            foreach (var (kind, text) in GetTokens().Concat(GetSeparators()))
            {
                yield return new object[] { kind, text };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsData()
        {
            foreach (var (t1kind, t1text, t2kind, t2text) in GetTokenPairs())
            {
                yield return new object[] { t1kind, t1text, t2kind, t2text };
            }
        }

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorData()
        {
            foreach (var (t1kind, t1text, sepKind, sepText, t2kind, t2text) in GetTokenPairsWithSeparator())
            {
                yield return new object[] { t1kind, t1text, sepKind, sepText, t2kind, t2text };
            }
        }

        private static IEnumerable<(SyntaxKind kind, string)> GetTokens()
        {
            var fixedTokens = Enum.GetValues(typeof(SyntaxKind))
                                .Cast<SyntaxKind>()
                                .Select(k => (kind: k, text: SyntaxFacts.GetText(k)))
                                .Where(t => t.text is not null);

            var dynamicTokens = new[]
            {
                (SyntaxKind.IdentifierToken, "a"),
                (SyntaxKind.IdentifierToken, "abc"),
                (SyntaxKind.IdentifierToken, "ihfijgdj"),
                (SyntaxKind.IdentifierToken, "JSHFIJDFH"),
                (SyntaxKind.NumberToken, "1"),
                (SyntaxKind.NumberToken, "243"),
                (SyntaxKind.NumberToken, "8374943"),
                (SyntaxKind.StringToken, "\"Hello, World!\""),
                (SyntaxKind.StringToken, "\"\\\"Sex money blaze it\\\"\""),
                (SyntaxKind.StringToken, "\"ZZZZ__()\""),
            };

            return fixedTokens.Concat(dynamicTokens);
        }

        private static IEnumerable<(SyntaxKind kind, string)> GetSeparators()
        {
            return new[]
            {
                (SyntaxKind.WhitespaceToken, " "),
                (SyntaxKind.WhitespaceToken, "  "),
                (SyntaxKind.WhitespaceToken, "\t"),
                (SyntaxKind.WhitespaceToken, "\n"),
                (SyntaxKind.WhitespaceToken, "\r"),
                (SyntaxKind.WhitespaceToken, "\n\r"),
            };
        }

        public static bool RequiresSeparator(SyntaxKind t1Kind, SyntaxKind t2Kind)
        {
            if (SyntaxFacts.IsWord(t1Kind) && SyntaxFacts.IsWord(t2Kind))
                return true;

            return (t1Kind, t2Kind) switch
            {
                (SyntaxKind.NumberToken, SyntaxKind.NumberToken) => true,
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.ExclamationToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.ExclamationToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.PipeToken, SyntaxKind.PipeToken) => true,
                (SyntaxKind.PipeToken, SyntaxKind.PipePipeToken) => true,
                (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandToken) => true,
                (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandAmpersandToken) => true,
                (SyntaxKind.LessThanToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.LessThanToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.LessOrEqualToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.LessOrEqualToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.GreaterThanToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.GreaterThanToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.GreaterOrEqualToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.GreaterOrEqualToken, SyntaxKind.EqualsEqualsToken) => true,
                _ => false,
            };
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text, SyntaxKind t2Kind, string t2Text)> GetTokenPairs()
        {
            foreach (var (t1kind, t1text) in GetTokens())
            {
                foreach (var (t2kind, t2text) in GetTokens())
                {
                    if (!RequiresSeparator(t1kind, t2kind))
                        yield return (t1kind, t1text, t2kind, t2text);
                }
            }
        }

        private static IEnumerable<(SyntaxKind t1Kind, string t1Text,
                                    SyntaxKind separatorKind, string separatorText,
                                    SyntaxKind t2Kind, string t2Text)> GetTokenPairsWithSeparator()
        {
            foreach (var (t1kind, t1text) in GetTokens())
            {
                foreach (var (sepKind, sepText) in GetSeparators())
                {
                    foreach (var (t2kind, t2text) in GetTokens())
                    {
                        yield return (t1kind, t1text, sepKind, sepText, t2kind, t2text);
                    }
                }
            }
        }
    }
}