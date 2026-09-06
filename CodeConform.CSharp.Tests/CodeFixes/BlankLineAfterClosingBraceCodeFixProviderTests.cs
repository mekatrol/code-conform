using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.CodeFixes;
using CodeConform.CSharp.Tests.TestInfrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.CodeFixes;

/// <summary>
/// Verifies CC0003 edits for executable and declaration closing braces.
/// </summary>
[TestClass]
public sealed class BlankLineAfterClosingBraceCodeFixProviderTests
{
    /// <summary>
    /// Verifies separation after an executable block.
    /// Expected result: a blank line is inserted before the following statement.
    /// </summary>
    [TestMethod]
    public async Task BlockBeforeStatementIsFixed()
    {
        var test = CreateTest("BlockBeforeStatement.cs.txt", "BlockBeforeStatement.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0003", DiagnosticSeverity.Warning)
                .WithSpan(9, 9, 9, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies separation between adjacent method declarations.
    /// Expected result: a blank line is inserted after the first method body.
    /// </summary>
    [TestMethod]
    public async Task MethodBeforeMethodIsFixed()
    {
        var test = CreateTest("MethodBeforeMethod.cs.txt", "MethodBeforeMethod.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0003", DiagnosticSeverity.Warning)
                .WithSpan(7, 5, 7, 6));

        await test.RunAsync();
    }

    private static CSharpCodeFixTest<
        BlankLineAfterClosingBraceAnalyzer,
        BlankLineAfterClosingBraceCodeFixProvider,
        DefaultVerifier> CreateTest(
        string sourceFixture,
        string fixedFixture)
    {
        return new CSharpCodeFixTest<
            BlankLineAfterClosingBraceAnalyzer,
            BlankLineAfterClosingBraceCodeFixProvider,
            DefaultVerifier>
        {
            TestCode = FixtureLoader.Load("BlankLineAfterClosingBrace", sourceFixture),
            FixedCode = FixtureLoader.Load("BlankLineAfterClosingBrace", fixedFixture)
        };
    }
}