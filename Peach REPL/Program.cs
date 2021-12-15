using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Syntax;
using Peach.CodeAnalysis.Text;

namespace Peach
{
    internal static class Program
    {
        private static readonly ConsoleColor DefaultColor = ConsoleColor.White;
        private static readonly ConsoleColor ErrorColour = ConsoleColor.Red;
        private static readonly ConsoleColor ExceptionColour = ConsoleColor.DarkRed;
        private static readonly ConsoleColor TreeColour = ConsoleColor.DarkGray;
        private static readonly ConsoleColor ResultColour = ConsoleColor.White;

        private static void Main()
        {
            var showTree = false;
            var variables = new Dictionary<VariableSymbol, object>();
            var textBuilder = new StringBuilder();

            for (; ; )
            {
                if (textBuilder.Length == 0)
                    ColourPrint(">", DefaultColor);
                else
                    ColourPrint("|", DefaultColor);

                var input = Console.ReadLine();

                var isBlank = string.IsNullOrWhiteSpace(input);

                if (textBuilder.Length == 0)
                {
                    if (isBlank)
                    {
                        break;
                    }
                    if (input == "#showTree")
                    {
                        showTree = !showTree;
                        ColourPrintln(showTree ? "Showing parse trees" : "Stopped showing parse trees", DefaultColor);
                        continue;
                    }

                    if (input == "#cls")
                    {
                        Console.Clear();
                        continue;
                    }
                }

                SyntaxTree syntaxTree;
                Compilation compilation;
                EvaluationResult result;

                try
                {
                    textBuilder.AppendLine(input);
                    var text = textBuilder.ToString();

                    syntaxTree = SyntaxTree.Parse(text);

                    if (!isBlank && syntaxTree.Diagnostics.Any())
                        continue;

                    compilation = new Compilation(syntaxTree);
                    result = compilation.Evaluate(variables);
                }
                catch (Exception e)
                {
                    ColourPrintln($"Exception thrown by interpreter: {e.Message}\n", ExceptionColour);
                    continue;
                }

                var diagnostics = result.Diagnostics;

                if (showTree)
                {
                    PrettyPrint(syntaxTree.Root);
                }
                if (!diagnostics.Any())
                {
                    ColourPrintln(result.Value, ResultColour);
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

                        ColourPrint(prefix, DefaultColor);
                        ColourPrint(error, ConsoleColor.Red);
                        ColourPrintln(suffix, DefaultColor);
                    }
                }

                textBuilder.Clear();
            }
        }

        private static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            ColourPrint(indent, TreeColour);
            ColourPrint(marker, TreeColour);
            ColourPrint(node.Kind, TreeColour);

            if (node is SyntaxToken t)
            {
                if (t.Value is not null)
                {
                    ColourPrint(" ");
                    ColourPrint(t.Value);
                }

                if (t.Kind == SyntaxKind.IdentifierToken)
                {
                    ColourPrint(" ");
                    ColourPrint(t.Text);
                }
            }

            ColourPrintln();

            indent += isLast ? "   " : "│  ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }

        private static void ColourPrint(object Str = null, ConsoleColor? Colour = null)
        {
            Console.ForegroundColor = Colour ?? DefaultColor;
            Console.Write(Str ?? "");
            Console.ResetColor();
        }

        private static void ColourPrintln(object Str = null, ConsoleColor? Colour = null)
        {
            Console.ForegroundColor = Colour ?? DefaultColor;
            Console.WriteLine(Str ?? "");
            Console.ResetColor();
        }
    }
}