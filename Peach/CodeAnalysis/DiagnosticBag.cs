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

        internal void ReportUnterminatedString(TextSpan span)
        {
            var message = GetUnterminatedStringErrorMesage();
            Report(span, message);
        }

        internal static string GetUnterminatedStringErrorMesage()
        {
            return "Unterminated string literal in input";
        }

        internal void ReportUnexpectedToken(TextSpan span, SyntaxKind kind, SyntaxKind expected)
        {
            string message = GetUnexpectedTokenErrorMessage(kind, expected);
            Report(span, message);
        }

        internal static string GetUnexpectedTokenErrorMessage(SyntaxKind kind, SyntaxKind expected)
        {
            return $"Unexpected token <{kind}>, expected <{expected}>";
        }

        internal void ReportUndefinedUnaryOperator(TextSpan span, string text, Type type)
        {
            string message = GetUndefinedUnaryOperatorErrorMessage(text, type);
            Report(span, message);
        }

        internal static string GetUndefinedUnaryOperatorErrorMessage(string text, Type type)
        {
            return $"Unary operator '{text}' is not defined for type {type}";
        }

        internal void ReportUndefinedBinaryOperator(TextSpan span, string text, Type left, Type right)
        {
            string message = GetUndefinedBinaryOperatorErrorMessage(text, left, right);
            Report(span, message);
        }

        internal static string GetUndefinedBinaryOperatorErrorMessage(string text, Type left, Type right)
        {
            return $"Binary operator '{text}' is not defined for types {left} and {right}";
        }

        internal void ReportUndefinedName(TextSpan span, string name)
        {
            string message = GetUndefinedNameErrorMessage(name);
            Report(span, message);
        }

        internal static string GetUndefinedNameErrorMessage(string name)
        {
            return $"Variable '{name}' is undefined";
        }

        internal void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            string message = GetVariableAlreadyDeclaredErrorMessage(name);
            Report(span, message);
        }

        internal static string GetVariableAlreadyDeclaredErrorMessage(string name)
        {
            return $"Cannot redeclare variable '{name}'";
        }

        internal void ReportCannotConvertTypes(TextSpan span, Type varType, Type expressionType)
        {
            string message = GetCannotConvertTypesErrorMessage(varType, expressionType);
            Report(span, message);
        }

        internal static string GetCannotConvertTypesErrorMessage(Type varType, Type expressionType)
        {
            return $"Cannot convert type {varType} to type {expressionType}";
        }

        internal void ReportCannotAssignToConst(TextSpan span, string name)
        {
            string message = GetCannotAssignToConstErrorMessage(name);
            Report(span, message);
        }

        internal static string GetCannotAssignToConstErrorMessage(string name)
        {
            return $"Cannot assign to constant variable '{name}'";
        }
    }
}