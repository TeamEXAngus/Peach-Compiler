using System;
using System.Collections.Generic;
using System.Linq;
using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Syntax;

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

            for (; ; )
            {
                ColourPrint(">", DefaultColor);
                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                    return;

                if (line == "#showTree")
                {
                    showTree = !showTree;
                    ColourPrintln(showTree ? "Showing parse trees" : "Stopped showing parse trees", DefaultColor);
                    continue;
                }

                if (line == "#cls")
                {
                    Console.Clear();
                    continue;
                }

                SyntaxTree syntaxTree;
                Compilation compilation;
                EvaluationResult result;

                try
                {
                    syntaxTree = SyntaxTree.Parse(line);
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
                        var character = diagnostic.Span.Start - text.Lines[lineIndex].Start + 1;

                        Console.WriteLine();
                        ColourPrintln($"Line {lineNumber}, pos {character} : {diagnostic}", ErrorColour);

                        var prefix = line.Substring(0, diagnostic.Span.Start);
                        var error = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                        var suffix = line[diagnostic.Span.End..];

                        ColourPrint(prefix, DefaultColor);
                        ColourPrint(error, ConsoleColor.Red);
                        ColourPrintln(suffix, DefaultColor);
                    }
                }
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