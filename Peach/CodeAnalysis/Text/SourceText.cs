using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Text
{
    public sealed class SourceText
    {
        private readonly string _text;

        private SourceText(string text)
        {
            _text = text;
            Lines = ParseLines(this, text);
        }

        public ImmutableArray<TextLine> Lines { get; }

        public char this[int pos] => _text[pos];

        public int Length => _text.Length;

        public int GetLineIndex(int pos)
        {
            var lower = 0;
            var upper = Lines.Length - 1;

            while (lower <= upper)
            {
                var index = lower + (upper - lower) / 2;
                var start = Lines[index].Start;

                if (pos == start)
                {
                    return index;
                }

                if (start > pos)
                {
                    upper = index - 1;
                }
                else
                {
                    lower = index + 1;
                }
            }

            return lower - 1;
        }

        private static ImmutableArray<TextLine> ParseLines(SourceText sourceText, string text)
        {
            var result = ImmutableArray.CreateBuilder<TextLine>();

            var pos = 0;
            var lineStart = 0;

            while (pos < text.Length)
            {
                var lineBreakWidth = GetLineBreakWidth(text, pos);

                if (lineBreakWidth == 0)
                {
                    pos++;
                }
                else
                {
                    result.AddLine(sourceText, pos, lineStart, lineBreakWidth);

                    pos += lineBreakWidth;
                    lineStart = pos;
                }
            }

            if (pos > lineStart)
                result.AddLine(sourceText, pos, lineStart, 0);

            return result.ToImmutable();
        }

        private static int GetLineBreakWidth(string text, int pos)
        {
            var curr = text[pos];
            var next = pos + 1 >= text.Length ? '\0' : text[pos + 1];

            if (curr == '\r' && next == '\n')
                return 2;

            if (curr == '\r' || curr == '\n')
                return 1;

            return 0;
        }

        public static SourceText From(string text)
        {
            return new SourceText(text);
        }

        public override string ToString() => _text;

        public string ToString(int start, int length) => _text.Substring(start, length);

        public string ToString(TextSpan span) => ToString(span.Start, span.Length);
    }

    internal static class LineArrayBuilder
    {
        internal static void AddLine(this ImmutableArray<TextLine>.Builder lineArrayBuilder, SourceText sourceText, int pos, int lineStart, int lineBreakWidth)
        {
            var lineLength = pos - lineStart;
            var line = new TextLine(sourceText, lineStart, lineLength, lineLength + lineBreakWidth);
            lineArrayBuilder.Add(line);
        }
    }
}