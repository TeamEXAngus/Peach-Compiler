using Peach.CodeAnalysis.Syntax;
using Peach.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Peach.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _diagnostics.GetEnumerator();

        internal void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        private void Report(TextSpan span, string message)
        {
            var diagnostic = new Diagnostic(span, message);
            _diagnostics.Add(diagnostic);
        }

        internal void ReportInvalidNumber(TextSpan span, string text, Type type)
        {
            var message = $"The number '{text}' isn't valid {type}";
            Report(span, message);
        }

        internal void ReportBadCharacter(int position, char current)
        {
            var span = new TextSpan(position, 1);
            var message = $"Bad character input: '{current}'";
            Report(span, message);
        }

        internal void ReportUnexpectedToken(TextSpan span, SyntaxKind kind, SyntaxKind expected)
        {
            var message = $"Unexpected token <{kind}>, expected <{expected}>";
            Report(span, message);
        }

        internal void ReportUndefinedUnaryOperator(TextSpan span, string text, Type type)
        {
            string message = ReportUndefinedUnaryOperatorMessage(text, type);
            Report(span, message);
        }

        internal static string ReportUndefinedUnaryOperatorMessage(string text, Type type)
        {
            return $"Unary operator '{text}' is not defined for type {type}";
        }

        internal void ReportUndefinedBinaryOperator(TextSpan span, string text, Type left, Type right)
        {
            string message = ReportUndefinedBinaryOperatorMessage(text, left, right);
            Report(span, message);
        }

        internal static string ReportUndefinedBinaryOperatorMessage(string text, Type left, Type right)
        {
            return $"Binary operator '{text}' is not defined for types {left} and {right}";
        }

        internal void ReportUndefinedName(TextSpan span, string name)
        {
            string message = ReportUndefinedNameMessage(name);
            Report(span, message);
        }

        internal static string ReportUndefinedNameMessage(string name)
        {
            return $"Variable '{name}' is undefined";
        }

        internal void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            string message = ReportVariableAlreadyDeclaredMessage(name);
            Report(span, message);
        }

        internal static string ReportVariableAlreadyDeclaredMessage(string name)
        {
            return $"Cannot redeclare variable '{name}'";
        }

        internal void ReportCannotConvertTypes(TextSpan span, Type varType, Type expressionType)
        {
            string message = ReportCannotConvertTypesMessage(varType, expressionType);
            Report(span, message);
        }

        internal static string ReportCannotConvertTypesMessage(Type varType, Type expressionType)
        {
            return $"Cannot convert type {varType} to type {expressionType}";
        }

        internal void ReportCannotAssignToConst(TextSpan span, string name)
        {
            string message = ReportCannotAssignToConstMessage(name);
            Report(span, message);
        }

        internal static string ReportCannotAssignToConstMessage(string name)
        {
            return $"Cannot assign to constant variable '{name}'";
        }
    }
}