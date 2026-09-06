using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.Tests.TestInfrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.Analyzers;

/// <summary>
/// Specifies CC0004 comment-block separation, grouping and exclusions for
/// first-item, documentation and end-of-line comments.
/// </summary>
[TestClass]
public sealed class BlankLineBeforeCommentAnalyzerTests
{
    /// <summary>
    /// Verifies that a line comment following an executable statement requires
    /// separation. Expected result: CC0004 spans the comment text.
    /// </summary>
    [TestMethod]
    public async Task CommentAfterStatementReportsDiagnostic()
    {
        var test = CreateTest("CommentAfterStatement.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 8, 39));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that adjacent line and block comments form one logical group.
    /// Expected result: one CC0004 diagnostic is reported on the first comment.
    /// </summary>
    [TestMethod]
    public async Task MixedCommentBlockReportsSingleDiagnostic()
    {
        var test = CreateTest("MixedCommentBlock.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 8, 30));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that three adjacent line comments are treated as one comment
    /// block. Expected result: one CC0004 diagnostic spans only the first line.
    /// </summary>
    [TestMethod]
    public async Task ConsecutiveLineCommentsReportSingleDiagnostic()
    {
        var test = CreateTest("ConsecutiveLineComments.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 8, 42));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a physical multiline block comment is analyzed as one
    /// trivia item without inspecting its interior asterisks as new comments.
    /// Expected result: one CC0004 diagnostic spans the complete block comment.
    /// </summary>
    [TestMethod]
    public async Task MultilineBlockCommentReportsSingleDiagnostic()
    {
        var test = CreateTest("MultilineBlockComment.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 11, 12));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies ownership when a comment block immediately follows a semantic
    /// closing brace. Expected result: CC0004 reports the first comment so the
    /// separator is placed before the entire explanatory group.
    /// </summary>
    [TestMethod]
    public async Task CommentAfterClosingBraceReportsDiagnostic()
    {
        var test = CreateTest("CommentAfterClosingBrace.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(11, 9, 11, 51));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies existing separators for consecutive line comments and a
    /// multiline block comment. Expected result: neither form produces CC0004.
    /// </summary>
    [TestMethod]
    public async Task SeparatedCommentFormsDoNotReportDiagnostic()
    {
        var test = CreateTest("SeparatedCommentForms.cs.txt");

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that first-item comments, XML documentation comments and
    /// end-of-line comments are outside CC0004's scope.
    /// Expected result: no CC0004 diagnostic is produced.
    /// </summary>
    [TestMethod]
    public async Task FirstAndDocumentationCommentsDoNotReportDiagnostic()
    {
        var test = CreateTest("FirstAndDocumentationComments.cs.txt");

        await test.RunAsync();
    }

    private static CSharpAnalyzerTest<
        BlankLineBeforeCommentAnalyzer,
        DefaultVerifier> CreateTest(string fixtureName)
    {
        return new CSharpAnalyzerTest<
            BlankLineBeforeCommentAnalyzer,
            DefaultVerifier>
        {
            TestCode = FixtureLoader.Load(
                "BlankLineBeforeComment",
                fixtureName)
        };
    }
}