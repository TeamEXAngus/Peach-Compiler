using System;
using System.Linq;
using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Syntax;
using Peach.CodeAnalysis.Binding;
using System.Collections.Generic;

namespace Peach
{
    internal class Program
    {
        private static void Main()
        {
            bool showTree = false;

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
                var binder = new Binder();
                var boundExpression = binder.BindExpression(syntaxTree.Root);

                IReadOnlyList<string> diagnostics = syntaxTree.Diagnostics.Concat(binder.Diagnostics).ToArray();

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    PrettyPrint(syntaxTree.Root);

                    Console.ResetColor();
                }
                if (diagnostics.Any())
                {
                    var e = new Evaluator(boundExpression);
                    Console.WriteLine(e.Evaluate());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    foreach (string Error in syntaxTree.Diagnostics)
                        Console.WriteLine(Error);

                    Console.ResetColor();
                }
            }
        }

        private static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            // ├──
            // │
            // └──

            var marker = isLast ? "└──" : "├──";

            Console.Write(indent);
            Console.Write(marker);
            Console.Write(node.Kind);

            if (node is SyntaxToken t && t.Value is not null)
            {
                Console.Write(" ");
                Console.Write(t.Value);
            }

            Console.WriteLine();

            indent += isLast ? "    " : "│   ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(child, indent, child == lastChild);
            }
        }
    }
}