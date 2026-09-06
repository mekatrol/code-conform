I am developing CodeConform:

https://github.com/mekatrol/code-conform

Repository locally:
D:\repos\code-conform

CodeConform is a Roslyn-based C# analyzer/code-fix project for enforcing custom
formatting rules that are not adequately covered by standard .editorconfig
formatters.

The overall formatting requirement is:

"Put a blank line before each control-flow statement that opens a block and
before each `return` statement, unless it is the first statement after an
opening brace. When a comment immediately documents the statement, put the
blank line before the comment instead. The repository format task enforces
this rule. Put a blank line after each closing brace unless it is followed
by another closing brace. Put a blank line before a comment block that
follows another statement, unless it is the first item after an opening
brace."

CURRENT STATE
=============

The blank-line-before-return rule is now implemented, including its analyzer,
code fix and test suite.

Diagnostic:
    CC0001

Implemented components:
    BlankLineBeforeReturnAnalyzer
    BlankLineBeforeReturnCodeFixProvider

The implementation handles:
- return as the first statement in a block: no blank line required
- return following another statement: blank line required
- void returns
- multiline return statements
- returns following control-flow/block statements
- multiple violations in the same document
- documenting comments immediately associated with a return
- single-line comment blocks
- multiple consecutive single-line comments
- multiline /* ... */ comments
- code fix places the blank line before the documenting comment block rather
  than between the comment and the return

The return analyzer and code fix have dedicated MSTest/Roslyn tests and
.cs.txt fixtures.

PROJECT STRUCTURE
=================

The repository uses a shallow structure:

CodeConform.CSharp.Analyzers/
    Rules/
    Syntax/
    BlankLineBeforeReturnAnalyzer.cs
    AnalyzerReleases.Shipped.md
    AnalyzerReleases.Unshipped.md
    CodeConform.CSharp.Analyzers.csproj

CodeConform.CSharp.CodeFixes/
    BlankLineBeforeReturnCodeFixProvider.cs
    CodeConform.CSharp.CodeFixes.csproj

CodeConform.CSharp.Tests/
    Analyzers/
    CodeFixes/
    Fixtures/
        BlankLineBeforeReturn/
    TestInfrastructure/
        FixtureLoader.cs
    CodeConform.CSharp.Tests.csproj

CodeConform.slnx
Directory.Build.props
Directory.Packages.props
.editorconfig

The analyzer and code-fix assemblies are deliberately separate:
- CodeConform.CSharp.Analyzers references compiler APIs only.
- CodeConform.CSharp.CodeFixes references Roslyn Workspaces.
- This separation avoids Roslyn RS1038.
- The NuGet PackageId remains CodeConform.CSharp.
- Both assemblies are packaged under analyzers/dotnet/cs.

TECHNOLOGY / CONVENTIONS
========================

Environment:
- Windows
- PowerShell
- Visual Studio 2026
- .NET SDK 10
- MSTest
- Roslyn analyzer/code-fix testing infrastructure

Production Roslyn projects target:
    netstandard2.0

Tests may target:
    net10.0

Central Package Management is enabled through:
    Directory.Packages.props

Tests use:
    Microsoft.CodeAnalysis.CSharp.Analyzer.Testing
    Microsoft.CodeAnalysis.CSharp.CodeFix.Testing
    DefaultVerifier
    MSTest

TEST CONVENTIONS
================

Tests are intended to act as specifications.

For every analyzer/code-fix behavior:
- Prefer explicit individual MSTest methods over DataRow for semantically
  different cases.
- Each test class must have XML documentation explaining its purpose.
- Each test method must have XML documentation explaining:
    - what behavior is being tested
    - the expected result
- Diagnostic-producing tests should explicitly contain:
    "CC0001" (or the applicable future diagnostic ID)
    DiagnosticSeverity
    WithSpan(...)
- Do not hide diagnostic IDs or expected behavior behind overly generic
  helper methods.
- Infrastructure setup may be factored into small CreateTest-style helpers.
- Use .cs.txt fixture files so fixture source is not compiled by the test
  project.
- Code-fix tests should reuse analyzer input fixtures where appropriate and
  add corresponding *.Fixed.cs.txt fixtures rather than duplicating inputs.
- Include many positive, negative and edge-case tests.
- When fixture contents change, diagnostic spans must be updated explicitly.

CODE STYLE
==========

The repository has a strict .editorconfig.

Important conventions when generating C#:
- 4-space indentation
- braces required
- file-scoped namespaces
- no top-level statements
- DO NOT use trailing commas in C# initializers or argument lists
- generated examples should conform to the repository formatting rules
- production classes/methods should have useful XML documentation
- important implementation logic should have comments explaining WHAT,
  WHY and PURPOSE rather than narrating obvious syntax

When telling me to edit something, give the exact repository-relative or
absolute file path.

Do not over-architect this. C# is the current focus and support for other
languages may never be implemented.

NEXT OBJECTIVE
==============

CC0001 / BlankLineBeforeReturn is complete.

I now want to implement the REMAINING parts of the original formatting
requirement systematically.

The remaining behavior includes:

1. Blank line BEFORE control-flow statements that open a block, unless the
   control-flow statement is the first statement after an opening brace.

   This needs careful definition of exactly which C# syntax constructs are
   included, for example:
   - if
   - for
   - foreach
   - while
   - do
   - switch
   - try
   - using statement
   - lock
   - possibly other block-opening constructs where appropriate

   We need to distinguish actual control-flow statements from constructs that
   merely contain braces.

2. COMMENT ASSOCIATION for those statements.

   Example:

       DoSomething();

       // Explain why this branch is needed.
       if (condition)
       {
       }

   NOT:

       DoSomething();
       // Explain why this branch is needed.

       if (condition)
       {
       }

   A contiguous documenting comment block belongs with the following
   statement.

3. Blank line AFTER each closing brace unless it is followed by another
   closing brace.

   This needs precise handling of C# constructs where tokens such as:
       else
       catch
       finally
       while (for do/while)
   legitimately follow a closing brace and should not be broken incorrectly.

   We need to define the rule semantically using Roslyn syntax rather than
   blindly processing `}` text.

4. Blank line BEFORE a comment block that follows another statement, unless
   the comment block is the first item after an opening brace.

   Example requiring a blank line:

       DoSomething();

       // Explanation of what happens next.
       DoSomethingElse();

   But no leading blank line should be required here:

       {
           // First item in this block.
           DoSomething();
       }

5. Interactions between all rules.

   The rules must not fight each other or produce multiple contradictory
   edits for the same whitespace.

6. IDE/code-fix behavior and Fix All.

7. Idempotency:

       Conform(Conform(source)) == Conform(source)

DESIGN REQUIREMENT
==================

Before implementing the next analyzer, help me decompose the remaining
requirement into a small, coherent set of Roslyn diagnostics/code-fix
providers.

I do NOT want one diagnostic for every individual C# keyword if several
constructs represent the same formatting rule.

At the same time, do not create one giant analyzer/code fix if separating
rules gives clearer diagnostics, tests and maintenance.

Shared syntax/trivia logic should go into the analyzer project's Syntax/
area where it genuinely prevents duplication.

Start by proposing:
1. the remaining diagnostic IDs and names,
2. exactly what syntax each diagnostic covers,
3. how the diagnostics interact,
4. the recommended implementation order,
5. the test categories required for each.

Do not start writing all implementation files immediately. First establish
and agree on the rule decomposition and edge-case semantics.