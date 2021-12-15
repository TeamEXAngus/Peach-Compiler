using Peach.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace Peach_Tests.CodeAnalysis
{
    internal sealed class AnnotatedText
    {
        public AnnotatedText(string text, ImmutableArray<TextSpan> spans)
        {
            Text = text;
            Spans = spans;
        }

        public string Text { get; }
        public ImmutableArray<TextSpan> Spans { get; }

        public static AnnotatedText Parse(string text)
        {
            text = Unindent(text);

            var stringBuilder = new StringBuilder();
            var arrayBuilder = ImmutableArray.CreateBuilder<TextSpan>();
            var startStack = new Stack<int>();

            var pos = 0;

            foreach (var c in text)
            {
                if (c == '[')
                {
                    startStack.Push(pos);
                }
                else if (c == ']')
                {
                    if (startStack.Count == 0)
                        throw new ArgumentException("Invalid ']' in text", nameof(text));

                    var startPos = startStack.Pop();
                    var span = TextSpan.FromBounds(startPos, pos);
                    arrayBuilder.Add(span);
                }
                else
                {
                    pos++;
                    stringBuilder.Append(c);
                }
            }

            if (startStack.Count != 0)
                throw new ArgumentException("Missing ']' in text", nameof(text));

            return new AnnotatedText(stringBuilder.ToString(), arrayBuilder.ToImmutable());
        }

        public static string Unindent(string text)
        {
            var lines = UnindentLines(text);
            return string.Join('\n', lines);
        }

        public static string[] UnindentLines(string text)
        {
            var lines = new List<string>();

            using (var reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) is not null)
                {
                    lines.Add(line);
                }
            }

            var minIndent = int.MaxValue;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                if (line.Trim().Length == 0)
                {
                    lines[i] = string.Empty;
                    continue;
                }

                var indent = line.Length - line.TrimStart().Length;
                minIndent = Math.Min(indent, minIndent);
            }

            while (lines.Count > 0 && lines[0].Length == 0)
                lines.RemoveAt(0);

            while (lines.Count > 0 && lines[^1].Length == 0)
                lines.RemoveAt(lines.Count - 1);

            return lines.ToArray();
        }
    }
}