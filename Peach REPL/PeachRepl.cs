using System;
using System.Collections.Generic;
using System.Linq;
using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Syntax;
using Peach.CodeAnalysis.Text;

namespace Peach
{
    internal class PeachRepl : Repl
    {
        protected override void EvaluteSubmission(string sourceText)
        {
            var syntaxTree = SyntaxTree.Parse(sourceText);

            var compilation = _previous is null ?
                                new Compilation(syntaxTree) :
                                _previous.ContinueWith(syntaxTree);

            EvaluationResult result;
            IEnumerable<Diagnostic> diagnostics;

            if (compilation.SyntaxTree.Diagnostics.Any())
            {
                ShowDiagnostics(syntaxTree.Text, syntaxTree.Diagnostics);
                return;
            }

            result = compilation.Evaluate(_variables);

            diagnostics = result.Diagnostics;

            if (_showTree)
                syntaxTree.Root.WriteTo(Console.Out);

            if (_showProgram)
                compilation.EmitTree(Console.Out);

            if (!diagnostics.Any())
            {
                if (result is not null)
                    ColourPrintln(result.Value, ResultColour);
                _previous = compilation;
            }
            else
            {
                ShowDiagnostics(syntaxTree.Text, diagnostics);
            }
        }

        private static void ShowDiagnostics(SourceText text, IEnumerable<Diagnostic> diagnostics)
        {
            foreach (var diagnostic in diagnostics)
            {
                var lineIndex = text.GetLineIndex(diagnostic.Span.Start);
                var lineNumber = lineIndex + 1;
                var line = text.Lines[lineIndex];
                var character = diagnostic.Span.Start - line.Start + 1;

                Console.WriteLine();
                ColourPrintln($"Line {lineNumber}, pos {character} : {diagnostic}", ErrorColour);

                var prefixSpan = TextSpan.FromBounds(line.Start, diagnostic.Span.Start);
                var suffixSpan = TextSpan.FromBounds(diagnostic.Span.End, line.End);

                var prefix = text.ToString(prefixSpan);
                var error = text.ToString(diagnostic.Span);
                var suffix = text.ToString(suffixSpan);

                ColourPrint(prefix);
                ColourPrint(error, ErrorColour);
                ColourPrintln(suffix);
            }
        }

        protected override void RenderLine(string line)
        {
            var tokens = SyntaxTree.ParseTokens(line);
            foreach (var token in tokens)
            {
                ColourPrint(token.Text, GetColourOf(token.Kind));
            }
        }

        protected override void EvaluateMetaCommand(string command)
        {
            switch (command)
            {
                case "#showTree":
                    _showTree = !_showTree;
                    ColourPrintln(_showTree ? "Showing parse trees" : "Stopped showing parse trees");
                    return;

                case "#showProgram":
                    _showProgram = !_showProgram;
                    ColourPrintln(_showProgram ? "Showing bound trees" : "Stopped showing bound trees");
                    return;

                case "#cls":
                    Console.Clear();
                    return;

                case "#reset":
                    _previous = null;
                    _variables.Clear();
                    ColourPrintln("Reset all variables and scopes");
                    return;

                case "#run":
                    var text = System.IO.File.ReadAllText(@"C:\Users\angus\code.pch");
                    EvaluteSubmission(text);
                    return;

                default:
                    base.EvaluateMetaCommand(command);
                    return;
            }
        }

        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            var lastTwoLinesAreBlank = text.Split('\n')
                                           .Reverse()
                                           .TakeWhile(s => string.IsNullOrEmpty(s))
                                           .Count() == 2;

            if (lastTwoLinesAreBlank)
                return true;

            var syntaxTree = SyntaxTree.Parse(text);

            if (syntaxTree.Root.Members.Last().GetLastToken().IsMissing)
                return false;

            return true;
        }

        private static ConsoleColor GetColourOf(SyntaxKind kind)
        {
            if (kind == SyntaxKind.StringToken)
                return ConsoleColor.Red;

            return SyntaxFacts.GetTokenKind(kind) switch
            {
                TokenKind.Identifier => ConsoleColor.DarkYellow,
                TokenKind.Keyword => ConsoleColor.Blue,
                TokenKind.Literal => ConsoleColor.DarkCyan,
                TokenKind.Operator => ConsoleColor.DarkGray,
                _ => ConsoleColor.White,
            };
        }
    }
}