﻿using System;
using System.Collections.Generic;

namespace Peach.CodeAnalysis.Syntax
{
    public static class SyntaxFacts
    {
        internal static int GetUnaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => 6,
                SyntaxKind.MinusToken => 6,
                SyntaxKind.ExclamationToken => 6,

                _ => 0
            };
        }

        internal static int GetBinaryOperatorPrecedence(this SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PipePipeToken => 1,

                SyntaxKind.AmpersandAmpersandToken => 2,

                SyntaxKind.EqualsEqualsToken => 3,
                SyntaxKind.ExclamationEqualsToken => 3,
                SyntaxKind.LessThanToken => 3,
                SyntaxKind.LessOrEqualToken => 3,
                SyntaxKind.GreaterThanToken => 3,
                SyntaxKind.GreaterOrEqualToken => 3,

                SyntaxKind.PlusToken => 4,
                SyntaxKind.MinusToken => 4,

                SyntaxKind.AsteriskToken => 5,
                SyntaxKind.SlashToken => 5,

                _ => 0
            };
        }

        internal static SyntaxKind GetKeywordKind(string text)
        {
            return text switch
            {
                "true" => SyntaxKind.TrueKeyword,
                "false" => SyntaxKind.FalseKeyword,
                "let" => SyntaxKind.LetKeyword,
                "const" => SyntaxKind.ConstKeyword,
                "if" => SyntaxKind.IfKeyword,
                "else" => SyntaxKind.ElseKeyword,
                "while" => SyntaxKind.WhileKeyword,
                "not" => SyntaxKind.NotKeyword,
                "for" => SyntaxKind.ForKeyword,
                "from" => SyntaxKind.FromKeyword,
                "to" => SyntaxKind.ToKeyword,
                "step" => SyntaxKind.StepKeyword,
                "loop" => SyntaxKind.LoopKeyword,
                _ => SyntaxKind.IdentifierToken
            };
        }

        public static IEnumerable<SyntaxKind> GetBinaryOperatorKinds()
        {
            var kinds = Enum.GetValues(typeof(SyntaxKind)) as SyntaxKind[];
            foreach (var kind in kinds)
            {
                if (GetBinaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }

        public static IEnumerable<SyntaxKind> GetUnaryOperatorKinds()
        {
            var kinds = Enum.GetValues(typeof(SyntaxKind)) as SyntaxKind[];
            foreach (var kind in kinds)
            {
                if (GetUnaryOperatorPrecedence(kind) > 0)
                    yield return kind;
            }
        }

        public static string GetText(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.PlusToken => "+",
                SyntaxKind.MinusToken => "-",
                SyntaxKind.AsteriskToken => "*",
                SyntaxKind.SlashToken => "/",
                SyntaxKind.AmpersandToken => "&",
                SyntaxKind.AmpersandAmpersandToken => "&&",
                SyntaxKind.PipeToken => "|",
                SyntaxKind.PipePipeToken => "||",
                SyntaxKind.ExclamationToken => "!",
                SyntaxKind.EqualsToken => "=",
                SyntaxKind.EqualsEqualsToken => "==",
                SyntaxKind.ExclamationEqualsToken => "!=",
                SyntaxKind.LessThanToken => "<",
                SyntaxKind.LessOrEqualToken => "<=",
                SyntaxKind.GreaterThanToken => ">",
                SyntaxKind.GreaterOrEqualToken => ">=",
                SyntaxKind.OpenParenToken => "(",
                SyntaxKind.CloseParenToken => ")",
                SyntaxKind.OpenBraceToken => "{",
                SyntaxKind.CloseBraceToken => "}",
                SyntaxKind.TrueKeyword => "true",
                SyntaxKind.FalseKeyword => "false",
                SyntaxKind.LetKeyword => "let",
                SyntaxKind.ConstKeyword => "const",
                SyntaxKind.IfKeyword => "if",
                SyntaxKind.ElseKeyword => "else",
                SyntaxKind.WhileKeyword => "while",
                SyntaxKind.NotKeyword => "not",
                SyntaxKind.ForKeyword => "for",
                SyntaxKind.FromKeyword => "from",
                SyntaxKind.ToKeyword => "to",
                SyntaxKind.StepKeyword => "step",
                SyntaxKind.LoopKeyword => "loop",
                _ => null,
            };
        }

        public static bool IsWord(SyntaxKind kind)
        {
            return kind switch
            {
                SyntaxKind.TrueKeyword => true,
                SyntaxKind.FalseKeyword => true,
                SyntaxKind.LetKeyword => true,
                SyntaxKind.ConstKeyword => true,
                SyntaxKind.IfKeyword => true,
                SyntaxKind.ElseKeyword => true,
                SyntaxKind.WhileKeyword => true,
                SyntaxKind.NotKeyword => true,
                SyntaxKind.IdentifierToken => true,
                SyntaxKind.ForKeyword => true,
                SyntaxKind.FromKeyword => true,
                SyntaxKind.ToKeyword => true,
                SyntaxKind.StepKeyword => true,
                SyntaxKind.LoopKeyword => true,
                _ => false,
            };
        }
    }
}