using Peach.CodeAnalysis.Syntax;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace Peach_Tests.CodeAnalysis.Syntax
{
    public class LexerTest
    {
        [Theory]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(SyntaxKind kind, string text)
        {
            var tokens = SyntaxTree.ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(kind, token.Kind);
            Assert.Equal(text, token.Text);
        }

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

        private static IEnumerable<(SyntaxKind, string)> GetTokens()
        {
            return new[]
            {
                (SyntaxKind.PlusToken, "+"),
                (SyntaxKind.MinusToken, "-"),
                (SyntaxKind.AsteriskToken, "*"),
                (SyntaxKind.SlashToken, "/"),
                (SyntaxKind.AmpersandToken, "&"),
                (SyntaxKind.AmpersandAmpersandToken, "&&"),
                (SyntaxKind.PipeToken, "|"),
                (SyntaxKind.PipePipeToken, "||"),
                (SyntaxKind.ExclamationToken, "!"),
                (SyntaxKind.EqualsToken, "="),
                (SyntaxKind.EqualsEqualsToken, "=="),
                (SyntaxKind.ExclamationEqualsToken, "!="),
                (SyntaxKind.OpenParenToken, "("),
                (SyntaxKind.CloseParenToken, ")"),
                (SyntaxKind.TrueKeyword, "true"),
                (SyntaxKind.FalseKeyword, "false"),

                (SyntaxKind.IdentifierToken, "a"),
                (SyntaxKind.IdentifierToken, "abc"),
                (SyntaxKind.IdentifierToken, "ihfijgdj"),
                (SyntaxKind.IdentifierToken, "JSHFIJDFH"),
                (SyntaxKind.NumberToken, "1"),
                (SyntaxKind.NumberToken, "243"),
                (SyntaxKind.NumberToken, "8374943"),
            };
        }

        private static IEnumerable<(SyntaxKind, string)> GetSeparators()
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
            return (t1Kind, t2Kind) switch
            {
                (SyntaxKind.IdentifierToken, SyntaxKind.IdentifierToken) => true,
                (SyntaxKind.IdentifierToken, SyntaxKind.TrueKeyword) => true,
                (SyntaxKind.IdentifierToken, SyntaxKind.FalseKeyword) => true,
                (SyntaxKind.TrueKeyword, SyntaxKind.IdentifierToken) => true,
                (SyntaxKind.TrueKeyword, SyntaxKind.TrueKeyword) => true,
                (SyntaxKind.TrueKeyword, SyntaxKind.FalseKeyword) => true,
                (SyntaxKind.FalseKeyword, SyntaxKind.IdentifierToken) => true,
                (SyntaxKind.FalseKeyword, SyntaxKind.TrueKeyword) => true,
                (SyntaxKind.FalseKeyword, SyntaxKind.FalseKeyword) => true,
                (SyntaxKind.NumberToken, SyntaxKind.NumberToken) => true,
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.ExclamationToken, SyntaxKind.EqualsToken) => true,
                (SyntaxKind.ExclamationToken, SyntaxKind.EqualsEqualsToken) => true,
                (SyntaxKind.PipeToken, SyntaxKind.PipeToken) => true,
                (SyntaxKind.PipeToken, SyntaxKind.PipePipeToken) => true,
                (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandToken) => true,
                (SyntaxKind.AmpersandToken, SyntaxKind.AmpersandAmpersandToken) => true,
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