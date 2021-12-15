using System;
using System.Collections.Generic;
using System.IO;
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
        private static readonly ConsoleColor PromptColour = ConsoleColor.Green;
        private static readonly ConsoleColor ErrorColour = ConsoleColor.Red;
        private static readonly ConsoleColor ExceptionColour = ConsoleColor.DarkRed;
        private static readonly ConsoleColor TreeColour = ConsoleColor.DarkGray;
        private static readonly ConsoleColor NodeColour = ConsoleColor.Cyan;
        private static readonly ConsoleColor TokenColour = ConsoleColor.Blue;
        private static readonly ConsoleColor ResultColour = ConsoleColor.Magenta;

        private static bool showTree = false;
        private static bool showProgram = false;
        private static readonly Dictionary<VariableSymbol, object> variables = new();
        private static readonly StringBuilder textBuilder = new();
        private static int indentLevel = 0;
        private static string[] inputLines = null;
        private static Compilation previous = null;

        private static readonly string DefaultFilePath = @"C:\Users\angus\code.pch";

        private static void Main()
        {
            for (; ; )
            {
                string input;
                bool isBlank;

                if (inputLines is null)
                {
                    if (textBuilder.Length == 0)
                        ColourPrint("» ", PromptColour);
                    else
                        ColourPrint("· " + GenerateIndent(indentLevel), PromptColour);

                    input = Console.ReadLine();

                    isBlank = string.IsNullOrWhiteSpace(input);
                }
                else
                {
                    input = "{" + string.Join('\n', inputLines) + "}";
                    inputLines = null;
                    isBlank = false;
                }

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

                    if (input == "#showProgram")
                    {
                        showProgram = !showProgram;
                        ColourPrintln(showProgram ? "Showing bound trees" : "Stopped showing bound trees", DefaultColor);
                        continue;
                    }

                    if (input == "#cls")
                    {
                        Console.Clear();
                        continue;
                    }

                    if (input == "#reset")
                    {
                        previous = null;
                        continue;
                    }

                    if (input.StartsWith("#run"))
                    {
                        if (input.Length <= 5)
                            input = DefaultFilePath;
                        else
                            input = input[5..];

                        try
                        {
                            inputLines = File.ReadAllLines(input);
                            foreach (var line in inputLines)
                                Console.WriteLine(line);
                        }
                        catch (Exception e)
                        {
                            ColourPrintln($"Exception when opening file {input} : {e.Message}", ExceptionColour);
                        }
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

                    if (input.Contains('{') && !input.Contains('}'))
                        indentLevel++;

                    if (!isBlank && syntaxTree.Diagnostics.Any())

                        continue;

                    indentLevel = 0;

                    compilation = previous is null ?
                                    new Compilation(syntaxTree) :
                                    previous.ContinueWith(syntaxTree);
                    result = compilation.Evaluate(variables);
                }
                catch (Exception e)
                {
                    ColourPrintln($"Exception thrown by interpreter: {e.Message}\n", ExceptionColour);
                    continue;
                }

                var diagnostics = result.Diagnostics;

                if (showTree)
                    PrettyPrint(syntaxTree.Root);

                if (showProgram)
                    compilation.EmitTree(Console.Out);

                if (!diagnostics.Any())
                {
                    if (inputLines is null)
                        ColourPrintln(result.Value, ResultColour);
                    previous = compilation;
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

        private static string GenerateIndent(int level)
        {
            var SB = new StringBuilder();

            for (int i = 0; i < level; i++)
                SB.Append('\t');

            return SB.ToString();
        }

        private static void PrettyPrint(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";

            ColourPrint(indent, TreeColour);
            ColourPrint(marker, TreeColour);
            ColourPrint(node.Kind, node is SyntaxToken ? TokenColour : NodeColour);

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