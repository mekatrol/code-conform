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

The complete blank-line rule family is implemented with analyzers, code fixes,
Fix All support and fixture-backed MSTest/Roslyn tests.

Diagnostics and components:
    CC0001 / BlankLineBeforeReturn
        BlankLineBeforeReturnAnalyzer
        BlankLineBeforeReturnCodeFixProvider

    CC0002 / BlankLineBeforeBlockStatement
        BlankLineBeforeBlockStatementAnalyzer
        BlankLineBeforeBlockStatementCodeFixProvider

    CC0003 / BlankLineAfterClosingBrace
        BlankLineAfterClosingBraceAnalyzer
        BlankLineAfterClosingBraceCodeFixProvider

    CC0004 / BlankLineBeforeComment
        BlankLineBeforeCommentAnalyzer
        BlankLineBeforeCommentCodeFixProvider

CC0001 handles expression and void returns, multiline returns, first-statement
exemptions, multiple violations, and documenting comment blocks.

CC0002 covers block-bodied if, for, foreach, while, do, switch, try, using,
lock and fixed statements. Unbraced statements, using declarations and
grammatical continuations such as else-if are excluded. A statement that is
first in its Roslyn statement list is exempt.

CC0003 covers semantic executable, namespace, type, enum and accessor-list
closing braces. It excludes expression-level braces and does not separate:
- consecutive closing braces
- else, catch or finally clauses
- the while clause of a do/while statement
- required semicolons
- the final closing brace at end of file

CC0004 covers ordinary single-line and multiline comment blocks. It excludes:
- XML documentation comments
- end-of-line comments
- directives
- comments that are the first item after an opening brace
- comments owned by CC0001 or CC0002

Comment association supports single `//` comments, consecutive `//` blocks,
multiline `/* ... */` blocks and mixed adjacent comment forms. Statement-specific
rules place their separator before the first documenting comment rather than
between the comment and statement.

Each physical whitespace boundary has one diagnostic owner:
1. CC0001 owns return boundaries.
2. CC0002 owns supported block-opening statement boundaries.
3. CC0004 owns remaining comment-leading boundaries.
4. CC0003 owns remaining post-brace boundaries.

This ownership order prevents duplicate diagnostics and contradictory Fix All
edits. Code fixes preserve the document's existing newline convention, and
fixed fixtures verify that another analyzer pass produces no further changes.

PROJECT STRUCTURE
=================

The repository uses a shallow structure:

CodeConform.CSharp.Analyzers/
    Rules/
    Syntax/
    BlankLineBeforeReturnAnalyzer.cs
    BlankLineBeforeBlockStatementAnalyzer.cs
    BlankLineAfterClosingBraceAnalyzer.cs
    BlankLineBeforeCommentAnalyzer.cs
    AnalyzerReleases.Shipped.md
    AnalyzerReleases.Unshipped.md
    CodeConform.CSharp.Analyzers.csproj

CodeConform.CSharp.CodeFixes/
    BlankLineBeforeReturnCodeFixProvider.cs
    BlankLineBeforeBlockStatementCodeFixProvider.cs
    BlankLineAfterClosingBraceCodeFixProvider.cs
    BlankLineBeforeCommentCodeFixProvider.cs
    FormattingCodeFix.cs
    CodeConform.CSharp.CodeFixes.csproj

CodeConform.CSharp.Validation/
    CodeConform.CSharp.Validation.csproj

CodeConform.CSharp.Tests/
    Analyzers/
    CodeFixes/
    Fixtures/
        BlankLineBeforeReturn/
        BlankLineBeforeBlockStatement/
        BlankLineAfterClosingBrace/
        BlankLineBeforeComment/
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
- The validation project recompiles both production source sets after the
  analyzer has been built so CC0001-CC0004 can fail a solution build for
  violations in analyzer sources as well as code-fix sources.

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

The four-rule formatting family is implemented. Continue by strengthening and
maintaining it rather than redesigning the diagnostic decomposition without a
specific reason.

For future changes:
- add explicit positive, negative and edge-case tests before considering a
  behavior complete
- use .cs.txt input and *.Fixed.cs.txt output fixtures
- cover plain statements, single comments, consecutive // comment blocks,
  multiline /* ... */ blocks and mixed comment groups where applicable
- test interactions to ensure only one diagnostic owns a whitespace boundary
- verify Fix All and idempotency
- update AnalyzerReleases.Unshipped.md and README.md when rule behavior changes
- keep shared syntax/trivia logic in Syntax/ only where it prevents genuine
  duplication
