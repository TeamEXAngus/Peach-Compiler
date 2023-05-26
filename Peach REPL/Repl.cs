using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Peach.CodeAnalysis;
using Peach.CodeAnalysis.Symbols;

namespace Peach
{
    internal abstract class Repl
    {
        public const ConsoleColor PromptColour = ConsoleColor.Green;
        public const ConsoleColor ErrorColour = ConsoleColor.Red;
        public const ConsoleColor ResultColour = ConsoleColor.Magenta;

        private readonly List<string> _submissionHistory = new();
        private int _submissionHistoryIndex = 0;

        protected Compilation _previous = null;
        protected Dictionary<VariableSymbol, object> _variables = new();

        protected bool _showTree = false;
        protected bool _showProgram = false;

        public void Run()
        {
            for (; ; )
            {
                var text = EditSubmission();
                if (string.IsNullOrEmpty(text))
                    return;

                if (!text.Contains('\n') && text.StartsWith('#'))
                    EvaluateMetaCommand(text);
                else
                    EvaluateSubmission(text);

                _submissionHistory.Add(text);
                _submissionHistoryIndex = 0;
            }
        }

        private sealed class SubmissionView
        {
            private readonly ObservableCollection<string> _submissionDocument;
            private readonly int _cursorTop;
            private int _renderedLineCount;
            private int _currentLineIndex;
            private int _currentCharIndex;

            private readonly Action<string> _renderLine;

            public SubmissionView(Action<string> renderLine, ObservableCollection<string> submissionDocument)
            {
                _renderLine = renderLine;
                _submissionDocument = submissionDocument;
                _submissionDocument.CollectionChanged += SubmissionDocumentChanged;
                _cursorTop = Console.CursorTop;
                Render();
            }

            private void SubmissionDocumentChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                Render();
            }

            private void Render()
            {
                Console.CursorVisible = false;

                var lineCount = 0;

                foreach (var line in _submissionDocument)
                {
                    Console.SetCursorPosition(0, _cursorTop + lineCount);

                    if (lineCount == 0)
                    {
                        ColourPrint("» ", PromptColour);
                    }
                    else
                    {
                        ColourPrint("· ", PromptColour);
                    }

                    _renderLine(line);
                    Console.WriteLine(new string(' ', Console.WindowWidth - line.Length));

                    lineCount++;
                }

                var numberOfBlankLines = _renderedLineCount - lineCount;
                if (numberOfBlankLines > 0)
                {
                    var blankLine = new string(' ', Console.WindowWidth);
                    for (int i = 0; i < numberOfBlankLines; i++)
                    {
                        Console.SetCursorPosition(0, _cursorTop + lineCount + i);
                        Console.WriteLine(blankLine);
                    }
                }

                _renderedLineCount = lineCount;
                Console.CursorVisible = true;
                UpdateCursorPosition();
            }

            private void UpdateCursorPosition()
            {
                Console.CursorTop = _cursorTop + _currentLineIndex;
                Console.CursorLeft = 2 + _currentCharIndex;
            }

            public int CurrentLineIndex
            {
                get => _currentLineIndex;
                set
                {
                    if (_currentLineIndex != value)
                    {
                        _currentLineIndex = value;
                        _currentCharIndex = Math.Min(_currentCharIndex, _submissionDocument[value].Length);
                        UpdateCursorPosition();
                    }
                }
            }

            public int CurrentCharIndex
            {
                get => _currentCharIndex;
                set
                {
                    if (_currentCharIndex != value)
                    {
                        _currentCharIndex = value;
                        UpdateCursorPosition();
                    }
                }
            }
        }

        private bool _done;

        private string EditSubmission()
        {
            _done = false;

            var document = new ObservableCollection<string>() { "" };
            var view = new SubmissionView(RenderLine, document);

            while (!_done)
            {
                var key = Console.ReadKey(true);
                HandleKey(key, view, document);
            }

            view.CurrentLineIndex = document.Count - 1;
            view.CurrentCharIndex = document[view.CurrentLineIndex].Length;

            Console.WriteLine();

            if (document.Count == 1 && document[0].Length == 0)
                return null;

            return string.Join('\n', document);
        }

        /*public void LoadString(string str)
        {
            var document = new ObservableCollection<string>() { "" };
            var view = new SubmissionView(RenderLine, document);

            foreach (char c in str)
            {
                HandleTyping(document, c.ToString(), view);
            }

            if (document.Count == 1 && document[0].Length == 0)
                return;

            EvaluteSubmission(string.Join('\n', document));
        }*/

        private void HandleKey(ConsoleKeyInfo key, SubmissionView view, ObservableCollection<string> document)
        {
            if (key.Modifiers == default)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        HandleEscape(document, view);
                        break;

                    case ConsoleKey.Enter:
                        HandleEnter(document, view);
                        break;

                    case ConsoleKey.LeftArrow:
                        HandleLeftArrow(document, view);
                        break;

                    case ConsoleKey.RightArrow:
                        HandleRightArrow(document, view);
                        break;

                    case ConsoleKey.UpArrow:
                        HandleUpArrow(document, view);
                        break;

                    case ConsoleKey.DownArrow:
                        HandleDownArrow(document, view);
                        break;

                    case ConsoleKey.Backspace:
                        HandleBackspace(document, view);
                        break;

                    case ConsoleKey.Delete:
                        HandleDelete(document, view);
                        break;

                    case ConsoleKey.Home:
                        HandleHome(document, view);
                        break;

                    case ConsoleKey.End:
                        HandleEnd(document, view);
                        break;

                    case ConsoleKey.Tab:
                        HandleTab(document, view);
                        break;

                    case ConsoleKey.PageUp:
                        HandlPageUp(document, view);
                        break;

                    case ConsoleKey.PageDown:
                        HandlePageDown(document, view);
                        break;

                    default:

                        break;
                }
            }
            else if (key.Modifiers == ConsoleModifiers.Control)
            {
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleControlEnter(document, view);
                        break;
                }
            }

            if (key.KeyChar >= ' ')
                HandleTyping(document, key.KeyChar.ToString(), view);
        }

        private static void HandleEscape(ObservableCollection<string> document, SubmissionView view)
        {
            document[view.CurrentLineIndex] = "";
            view.CurrentCharIndex = 0;
        }

        private void HandleEnter(ObservableCollection<string> document, SubmissionView view)
        {
            var documentText = string.Join('\n', document);
            if (documentText.StartsWith('#') || IsCompleteSubmission(documentText))
            {
                _done = true;
                return;
            }

            InsertLine(document, view);
        }

        private static void InsertLine(ObservableCollection<string> document, SubmissionView view)
        {
            var remainder = document[view.CurrentLineIndex][view.CurrentCharIndex..];
            document[view.CurrentLineIndex] = document[view.CurrentLineIndex][..view.CurrentCharIndex];

            var lineIndex = view.CurrentLineIndex + 1;
            document.Insert(lineIndex, remainder);
            view.CurrentCharIndex = 0;
            view.CurrentLineIndex = document.Count - 1;
        }

        private static void HandleControlEnter(ObservableCollection<string> document, SubmissionView view)
        {
            InsertLine(document, view);
        }

        private static void HandleLeftArrow(ObservableCollection<string> _, SubmissionView view)
        {
            if (view.CurrentCharIndex > 0)
                view.CurrentCharIndex--;
        }

        private static void HandleRightArrow(ObservableCollection<string> document, SubmissionView view)
        {
            var line = document[view.CurrentLineIndex];
            if (view.CurrentCharIndex < line.Length)
                view.CurrentCharIndex++;
        }

        private static void HandleUpArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLineIndex > 0)
                view.CurrentLineIndex--;
        }

        private static void HandleDownArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLineIndex < (document.Count - 1))
                view.CurrentLineIndex++;
        }

        private static void HandleBackspace(ObservableCollection<string> document, SubmissionView view)
        {
            var start = view.CurrentCharIndex;
            if (start == 0)
            {
                MergeLines(document, view);
                return;
            }

            var lineIndex = view.CurrentLineIndex;
            var line = document[lineIndex];

            var before = line.Substring(0, start - 1);
            var after = line[start..];
            document[lineIndex] = before + after;
            view.CurrentCharIndex--;
        }

        private static void MergeLines(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLineIndex == 0)
                return;

            var currentLine = document[view.CurrentLineIndex];
            var previousLine = document[view.CurrentLineIndex - 1];
            document.RemoveAt(view.CurrentLineIndex--);
            document[view.CurrentLineIndex] = previousLine + currentLine;
            view.CurrentCharIndex = previousLine.Length;
        }

        private static void HandleDelete(ObservableCollection<string> document, SubmissionView view)
        {
            var lineIndex = view.CurrentLineIndex;
            var line = document[lineIndex];
            var start = view.CurrentCharIndex;
            if (start >= line.Length)
                return;

            var before = line.Substring(0, start);
            var after = line[(start + 1)..];
            document[lineIndex] = before + after;
        }

        private static void HandleEnd(ObservableCollection<string> _, SubmissionView view)
        {
            view.CurrentCharIndex = 0;
        }

        private static void HandleHome(ObservableCollection<string> document, SubmissionView view)
        {
            view.CurrentCharIndex = document[view.CurrentLineIndex].Length;
        }

        private static void HandleTab(ObservableCollection<string> document, SubmissionView view)
        {
            const int TabWidth = 4;
            var start = view.CurrentCharIndex;
            var remaining = TabWidth - start % TabWidth;

            var line = document[view.CurrentLineIndex];
            document[view.CurrentLineIndex] = line.Insert(start, new string(' ', remaining));
            view.CurrentCharIndex += remaining;
        }

        private void HandlePageDown(ObservableCollection<string> document, SubmissionView view)
        {
            _submissionHistoryIndex--;
            if (_submissionHistoryIndex < 0)
                _submissionHistoryIndex = _submissionHistory.Count - 1;

            UpdateDocumentFromHistory(document, view);
        }

        private void HandlPageUp(ObservableCollection<string> document, SubmissionView view)
        {
            _submissionHistoryIndex++;
            if (_submissionHistoryIndex >= _submissionHistory.Count)
                _submissionHistoryIndex = 0;

            UpdateDocumentFromHistory(document, view);
        }

        private void UpdateDocumentFromHistory(ObservableCollection<string> document, SubmissionView view)
        {
            document.Clear();

            var historyItem = _submissionHistory[_submissionHistoryIndex];
            var lines = historyItem.Split('\n');
            foreach (var line in lines)
                document.Add(line);

            view.CurrentLineIndex = document.Count - 1;
            view.CurrentCharIndex = document[view.CurrentLineIndex].Length;
        }

        private static void HandleTyping(ObservableCollection<string> document, string text, SubmissionView view)
        {
            var lineIndex = view.CurrentLineIndex;
            var start = view.CurrentCharIndex;
            document[lineIndex] = document[lineIndex].Insert(start, text);
            view.CurrentCharIndex += text.Length;
        }

        protected static void ColourPrint(object text, ConsoleColor colour = ConsoleColor.White)
        {
            Console.ForegroundColor = colour;
            Console.Write(text);
            Console.ResetColor();
        }

        protected static void ColourPrintln(object text, ConsoleColor colour = ConsoleColor.White)
        {
            Console.ForegroundColor = colour;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        protected void ClearHistory()
        {
            _submissionHistory.Clear();
        }

        protected virtual void RenderLine(string line)
        {
            Console.Write(line);
        }

        protected virtual void EvaluateMetaCommand(string command)
        {
            ColourPrintln($"Invalid command '{command}'", ConsoleColor.Red);
        }

        protected abstract void EvaluateSubmission(string sourceText);

        protected abstract void EvaluateFile(string FileName);

        protected abstract bool IsCompleteSubmission(string text);
    }
}