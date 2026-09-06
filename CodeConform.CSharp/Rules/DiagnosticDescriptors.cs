using Microsoft.CodeAnalysis;

namespace CodeConform.CSharp.Rules;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor BlankLineBeforeReturn = new(
        DiagnosticIds.BlankLineBeforeReturn,
        "Blank line required before return statement",
        "A blank line is required before this return statement",
        "Formatting",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}