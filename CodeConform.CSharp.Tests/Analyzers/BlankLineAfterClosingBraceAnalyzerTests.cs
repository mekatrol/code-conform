using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.Tests.TestInfrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.Analyzers;

/// <summary>
/// Specifies CC0003 for semantic closing braces and its grammatical
/// continuation and nested-closing-brace exceptions.
/// </summary>
[TestClass]
public sealed class BlankLineAfterClosingBraceAnalyzerTests
{
    /// <summary>
    /// Verifies that an executable block followed by an ordinary statement
    /// requires separation. Expected result: CC0003 identifies the block's
    /// closing brace.
    /// </summary>
    [TestMethod]
    public async Task BlockBeforeStatementReportsDiagnostic()
    {
        var test = CreateTest("BlockBeforeStatement.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0003", DiagnosticSeverity.Warning)
                .WithSpan(9, 9, 9, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that declaration bodies are also semantic brace boundaries.
    /// Expected result: CC0003 identifies the first method's closing brace
    /// when another member follows immediately.
    /// </summary>
    [TestMethod]
    public async Task MethodBeforeMethodReportsDiagnostic()
    {
        var test = CreateTest("MethodBeforeMethod.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0003", DiagnosticSeverity.Warning)
                .WithSpan(7, 5, 7, 6));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that else, catch, finally and do/while remain joined to their
    /// preceding bodies and that consecutive closing braces remain joined.
    /// Expected result: no CC0003 diagnostic is produced.
    /// </summary>
    [TestMethod]
    public async Task ContinuationsAndNestedBracesDoNotReportDiagnostic()
    {
        var test = CreateTest("ContinuationsAndNestedBraces.cs.txt");

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that CC0003 yields ownership when an ordinary comment block
    /// follows a closing brace. Expected result: no CC0003 diagnostic is
    /// produced because CC0004 owns the same whitespace boundary.
    /// </summary>
    [TestMethod]
    public async Task CommentAfterClosingBraceDoesNotReportDiagnostic()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeComment",
            "CommentAfterClosingBrace.cs.txt");
        var test = new CSharpAnalyzerTest<
            BlankLineAfterClosingBraceAnalyzer,
            DefaultVerifier>
        {
            TestCode = source
        };

        await test.RunAsync();
    }

    private static CSharpAnalyzerTest<
        BlankLineAfterClosingBraceAnalyzer,
        DefaultVerifier> CreateTest(string fixtureName)
    {
        return new CSharpAnalyzerTest<
            BlankLineAfterClosingBraceAnalyzer,
            DefaultVerifier>
        {
            TestCode = FixtureLoader.Load(
                "BlankLineAfterClosingBrace",
                fixtureName)
        };
    }
}