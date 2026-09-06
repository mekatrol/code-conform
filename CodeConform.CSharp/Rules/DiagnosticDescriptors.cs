using Microsoft.CodeAnalysis;

namespace CodeConform.CSharp.Rules;

internal static class DiagnosticDescriptors
{
    public const string BlankLineBeforeReturnId = "CC0001";

    public static readonly DiagnosticDescriptor BlankLineBeforeReturn = new(
        BlankLineBeforeReturnId,
        "Blank line required before return statement",
        "A blank line is required before this return statement",
        "Formatting",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);
}