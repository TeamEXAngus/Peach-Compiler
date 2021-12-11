using System;
using System.Linq;
using Peach.CodeAnalysis;

namespace Peach
{
    internal class Program
    {
        private static void Main(string[] args)
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

                var parser = new Parser(line);
                var syntaxTree = parser.Parse();

                var defaultColor = Console.ForegroundColor;

                if (showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                    PrettyPrint(syntaxTree.Root);

                    Console.ForegroundColor = defaultColor;
                }
                if (!syntaxTree.Diagnostics.Any())
                {
                    var e = new Evaluator(syntaxTree.Root);
                    Console.WriteLine(e.Evaluate());
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;

                    foreach (string Error in syntaxTree.Diagnostics)
                        Console.WriteLine(Error);

                    Console.ForegroundColor = defaultColor;
                }
            }
        }

        public static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
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