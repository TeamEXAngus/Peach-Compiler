using System;
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

            var result = compilation.Evaluate(_variables);

            var diagnostics = result.Diagnostics;

            if (_showTree)
                syntaxTree.Root.WriteTo(Console.Out);

            if (_showProgram)
                compilation.EmitTree(Console.Out);

            if (!diagnostics.Any())
            {
                ColourPrintln(result.Value, ResultColour);
                _previous = compilation;
            }
            else
            {
                var text = syntaxTree.Text;

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

                    ColourPrint(prefix, ErrorColour);
                    ColourPrint(error, ErrorColour);
                    ColourPrintln(suffix, ErrorColour);
                }
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

                default:
                    base.EvaluateMetaCommand(command);
                    return;
            }
        }

        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            var syntaxTree = SyntaxTree.Parse(text);

            if (GetLastToken(syntaxTree.Root.Statement).IsMissing)
                return false;

            return true;
        }

        private SyntaxToken GetLastToken(SyntaxNode node)
        {
            if (node is SyntaxToken token)
                return token;

            return GetLastToken(node.GetChildren().Last());
        }

        private static ConsoleColor GetColourOf(SyntaxKind kind)
        {
            return SyntaxFacts.GetTokenKind(kind) switch
            {
                TokenKind.Identifier => ConsoleColor.Yellow,
                TokenKind.Keyword => ConsoleColor.Blue,
                TokenKind.Literal => ConsoleColor.Cyan,
                TokenKind.Operator => ConsoleColor.Gray,
                _ => ConsoleColor.White,
            };
        }
    }
}