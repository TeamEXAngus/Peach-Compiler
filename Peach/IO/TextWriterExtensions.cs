using Peach.CodeAnalysis.Syntax;
using System;
using System.CodeDom.Compiler;
using System.IO;

namespace Peach.IO
{
    internal static class TextWriterExtensions
    {
        public static bool IsConsoleOut(this TextWriter writer)
        {
            return writer == Console.Out ||
                  (writer is IndentedTextWriter iw && iw.InnerWriter.IsConsoleOut());
        }

        public static void SetColour(this TextWriter writer, ConsoleColor color)
        {
            if (writer.IsConsoleOut())
                Console.ForegroundColor = color;
        }

        public static void ResetColour(this TextWriter writer)
        {
            if (writer.IsConsoleOut())
                Console.ResetColor();
        }

        public static void WriteSpace(this TextWriter writer)
            => writer.Write(" ");

        public static void WriteKeyword(this TextWriter writer, SyntaxKind kind)
            => WriteKeyword(writer, SyntaxFacts.GetText(kind));

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.Blue);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WriteGotoLabel(this TextWriter writer, SyntaxKind kind)
            => WriteGotoLabel(writer, SyntaxFacts.GetText(kind));

        public static void WriteGotoLabel(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.Magenta);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WriteIdentifier(this TextWriter writer, SyntaxKind kind)
            => WriteIdentifier(writer, SyntaxFacts.GetText(kind));

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.DarkYellow);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WriteNumber(this TextWriter writer, SyntaxKind kind)
            => WriteNumber(writer, SyntaxFacts.GetText(kind));

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.DarkCyan);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WriteString(this TextWriter writer, SyntaxKind kind)
            => WriteString(writer, SyntaxFacts.GetText(kind));

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.Red);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WritePunctuation(this TextWriter writer, SyntaxKind kind)
            => WritePunctuation(writer, SyntaxFacts.GetText(kind));

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.DarkGray);
            writer.Write(text);
            writer.ResetColour();
        }
    }
}