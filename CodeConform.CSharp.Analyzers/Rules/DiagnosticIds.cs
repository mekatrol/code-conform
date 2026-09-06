namespace CodeConform.CSharp.Analyzers.Rules;

/// <summary>
/// Defines the diagnostic identifiers exposed by CodeConform C# rules.
/// </summary>
public static class DiagnosticIds
{
    /// <summary>
    /// Identifies the rule requiring a blank line before a return statement.
    /// </summary>
    public const string BlankLineBeforeReturn = "CC0001";

    /// <summary>
    /// Identifies the rule requiring a blank line before a block-opening statement.
    /// </summary>
    public const string BlankLineBeforeBlockStatement = "CC0002";

    /// <summary>
    /// Identifies the rule requiring a blank line after a semantic closing brace.
    /// </summary>
    public const string BlankLineAfterClosingBrace = "CC0003";

    /// <summary>
    /// Identifies the rule requiring a blank line before a comment block.
    /// </summary>
    public const string BlankLineBeforeComment = "CC0004";
}