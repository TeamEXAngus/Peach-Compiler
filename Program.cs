using System;
using System.Linq;
using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Syntax;
using Peach.CodeAnalysis.Binding;
using System.Collections.Generic;

namespace Peach
{
    internal static class Program
    {
        private static void Main()
        {
            var showTree = false;

            for (; ; )
            {
                Console.Write(">");
                var line = Console.ReadLine();

                if (string.IsNullOrEmpty(line))
                    return;

                if (line == "#showTree")
                {
                    showTree = !showTree;
                    Console.WriteLine(showTree ? "Showing parse trees" : "Stopped showing parse trees");
                    continue;
                }

                if (line == "#cls")
                {
                    Console.Clear();
                    continue;
                }

                var syntaxTree = SyntaxTree.Parse(line);
                var compilation = new Compilation(syntaxTree);
                var result = compilation.Evaluate();

                var diagnostics = result.Diagnostics;

                if (showTree)
                {
                    PrettyPrint(syntaxTree.Root);
                }
                if (!diagnostics.Any())
                {
                    Console.WriteLine(result.Value);
                }
                else
                {
                    foreach (var diagnostic in diagnostics)
                    {
                        Console.WriteLine();
                        ColourPrintln(diagnostic, ConsoleColor.Red);

                        var prefix = line.Substring(0, diagnostic.Span.Start);
                        var error = line.Substring(diagnostic.Span.Start, diagnostic.Span.Length);
                        var suffix = line.Substring(diagnostic.Span.End);

                        ColourPrint("    " + prefix, ConsoleColor.DarkYellow);
                        ColourPrint(error, ConsoleColor.Red);
                        ColourPrint(suffix + "\n\n", ConsoleColor.DarkYellow);
                    }
                }
            }
        }

        private static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            ColourPrint(indent, ConsoleColor.DarkGray);
            ColourPrint(marker, ConsoleColor.DarkGray);
            ColourPrint(node.Kind, ConsoleColor.DarkGray);

            if (node is SyntaxToken t && t.Value is not null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();

            indent += isLast ? "   " : "│  ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }

        private static void ColourPrint(object Str, ConsoleColor Colour)
        {
            Console.ForegroundColor = Colour;
            Console.Write(Str);
            Console.ResetColor();
        }

        private static void ColourPrintln(object Str, ConsoleColor Colour)
        {
            Console.ForegroundColor = Colour;
            Console.WriteLine(Str);
            Console.ResetColor();
        }
    }
}