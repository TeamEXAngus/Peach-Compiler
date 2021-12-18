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

        public static void WriteKeyword(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.Blue);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WriteGotoLabel(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.Magenta);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WriteIdentifier(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.DarkYellow);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WriteNumber(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.DarkCyan);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WriteString(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.Red);
            writer.Write(text);
            writer.ResetColour();
        }

        public static void WritePunctuation(this TextWriter writer, string text)
        {
            writer.SetColour(ConsoleColor.DarkGray);
            writer.Write(text);
            writer.ResetColour();
        }
    }
}