using Microsoft.CodeAnalysis;

namespace CodeConform.CSharp.Analyzers.Rules;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor BlankLineBeforeReturn = new(
        DiagnosticIds.BlankLineBeforeReturn,
        "Blank line required before return statement",
        "A blank line is required before this return statement",
        "Formatting",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BlankLineBeforeBlockStatement = new(
        DiagnosticIds.BlankLineBeforeBlockStatement,
        "Blank line required before block-opening statement",
        "A blank line is required before this block-opening statement",
        "Formatting",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BlankLineAfterClosingBrace = new(
        DiagnosticIds.BlankLineAfterClosingBrace,
        "Blank line required after closing brace",
        "A blank line is required after this closing brace",
        "Formatting",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor BlankLineBeforeComment = new(
        DiagnosticIds.BlankLineBeforeComment,
        "Blank line required before comment block",
        "A blank line is required before this comment block",
        "Formatting",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}