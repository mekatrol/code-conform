# code-conform

`code-conform` provides additional C# code formatting and linting rules that complement the standard .NET and Roslyn tooling.

The project is intended to enforce coding standards that are not currently supported by tools such as `dotnet format`, without duplicating rules already provided by the .NET toolchain.

## Goals

* Extend existing .NET formatting and analysis tooling rather than replace it.
* Provide deterministic, syntax-aware C# rules using Roslyn.
* Detect coding-standard violations while writing code in Visual Studio.
* Provide automatic code fixes where practical.
* Support repository-wide validation and formatting.
* Integrate with existing `.editorconfig` and Roslyn diagnostic configuration.
* Be suitable for local development and CI environments.
* Operate entirely offline once dependencies have been restored.

## Formatting Rules

### Blank-Line Rules

The intended formatting rules are:

> Put a blank line before each control-flow statement that opens a block and before each `return` statement, unless it is the first statement after an opening brace.
>
> When a comment immediately documents the statement, put the blank line before the comment instead.
>
> Put a blank line after each closing brace unless it is followed by another closing brace.
>
> Put a blank line before a comment block that follows another statement, unless it is the first item after an opening brace.

For example:

```csharp
if (condition)
{
    DoSomething();

    if (otherCondition)
    {
        DoSomethingElse();
    }

    // Explain why the result is returned here.
    return result;
}
```

Rather than:

```csharp
if (condition)
{
    DoSomething();
    if (otherCondition)
    {
        DoSomethingElse();
    }
    // Explain why the result is returned here.
    return result;
}
```

## Philosophy

`code-conform` should only implement rules that cannot reasonably be enforced by existing standard tooling.

Where a rule can be configured using:

* `dotnet format`
* Roslyn/.NET analyzers
* `.editorconfig`
* built-in compiler diagnostics

the existing mechanism should be preferred.

`code-conform` exists to fill the gaps.

## Architecture

The repository is intentionally small and uses a shallow project structure:

```text
code-conform/
├── CodeConform.CSharp.Analyzers/
├── CodeConform.CSharp.CodeFixes/
├── CodeConform.CSharp.Validation/
├── CodeConform.CSharp.Tests/
├── .editorconfig
├── .gitignore
├── CodeConform.slnx
├── Directory.Build.props
├── Directory.Packages.props
├── LICENSE
└── README.md
```

### CodeConform.CSharp.Analyzers

Contains the compiler-safe Roslyn implementation:

* diagnostic analyzers
* formatting rules
* syntax and trivia helpers
* analyzer release tracking

The same rule implementation should be used by IDE analysis and command-line tooling to ensure consistent behaviour.

The project targets `netstandard2.0` to provide broad compatibility with Roslyn analyzer hosts.

### CodeConform.CSharp.CodeFixes

Contains the Roslyn Workspaces-based code-fix providers. Keeping code fixes in
a separate assembly prevents the analyzer assembly from taking a Workspaces
dependency and avoids Roslyn rule RS1038.

Both assemblies are packaged under `analyzers/dotnet/cs` in the
`CodeConform.CSharp` NuGet package.

### CodeConform.CSharp.Validation

Compiles the production analyzer and code-fix sources after the analyzer
assembly has been created, then runs CC0001–CC0004 against those sources. This
clean-build-safe validation step lets the solution enforce its own formatting
rules even though an analyzer cannot execute during the compilation that
creates that same analyzer assembly.

### CodeConform.CSharp.Tests

Contains analyzer and code-fix tests.

Formatting behaviour should be thoroughly tested against normal and unusual C# syntax before rules are considered stable.

Tests use source fixtures so that valid and intentionally invalid formatting can be represented independently of the formatting rules applied to the test project itself.

### CodeConform.Tool

A command-line tool may be added in the future for repository-wide checking and fixing.

The intended command would be:

```console
dotnet conform
```

Additional commands may include:

```console
dotnet conform check
dotnet conform fix
```

The command-line tool is secondary to the Roslyn analyzer implementation and is not currently required during the initial development phase.

## Visual Studio Integration

`CodeConform.CSharp.Analyzers` and `CodeConform.CSharp.CodeFixes` are distributed
together as the `CodeConform.CSharp` NuGet analyzer package.

Projects referencing the analyzer package receive diagnostics directly while editing and building C# code.

For example:

```csharp
DoSomething();
return result;
```

produces:

```text
CC0001: A blank line is required before this return statement.
```

The correctly formatted code is:

```csharp
DoSomething();

return result;
```

Where practical, diagnostics will also provide automatic code fixes.

## Configuration

Rule severity uses the standard Roslyn `.editorconfig` mechanism.

For example:

```ini
[*.cs]

dotnet_diagnostic.CC0001.severity = error
```

Supported severity values include:

```text
none
silent
suggestion
warning
error
```

Using the standard Roslyn configuration mechanism allows repositories to enable, disable, or change the severity of individual CodeConform rules without introducing a separate configuration system.

For example, configuring:

```ini
dotnet_diagnostic.CC0001.severity = error
```

causes a `CC0001` violation to fail `dotnet build`.

## `dotnet format`

`code-conform` is intended to work alongside `dotnet format`, not replace it.

A repository formatting workflow may eventually be:

```console
dotnet format
dotnet conform
```

Where possible, CodeConform analyzers and code fixes may also be executable through the analyzer support provided by `dotnet format`.

## Rule IDs

CodeConform diagnostics use the `CC` prefix.

Currently implemented rules are:

| Rule     | Description                                             |
| -------- | ------------------------------------------------------- |
| `CC0001` | Blank line required before a `return` statement          |
| `CC0002` | Blank line required before a block-opening statement     |
| `CC0003` | Blank line required after a semantic closing brace       |
| `CC0004` | Blank line required before an ordinary comment block     |

CC0002 covers block-bodied `if`, `for`, `foreach`, `while`, `do`, `switch`,
`try`, `using`, `lock`, and `fixed` statements. It does not apply to unbraced
embedded statements, using declarations, or grammatical continuations such as
`else if`.

CC0003 recognizes syntax-tree brace ownership instead of processing every `}`
character. It preserves `else`, `catch`, `finally`, the `while` clause of a
do/while statement, required semicolons, and adjacent closing braces.

CC0004 applies to ordinary `//` and `/* ... */` comment blocks. XML
documentation, end-of-line comments, directives, and comments that are the
first item after an opening brace are excluded.

When comments immediately document a CC0001 return or CC0002 block-opening
statement, that statement-specific diagnostic owns the boundary and its fix
inserts the blank line before the first comment. When a comment follows a
closing brace, CC0004 owns the boundary instead of CC0003. This ensures that
only one diagnostic and one edit apply to any whitespace boundary.

Rule IDs and definitions may change while the project is under initial development.

## Design Principles

### Deterministic

Running a formatter repeatedly should not continue changing the source:

```text
Conform(Conform(source)) == Conform(source)
```

### Syntax-aware

C# source is analysed using Roslyn syntax trees and trivia rather than regular expressions or simple line-oriented source manipulation.

### Semantics preserving

Automatic fixes should modify formatting and trivia without changing the meaning of the program.

### Minimal

Do not implement functionality already adequately provided by standard .NET tooling.

### Testable

Every formatting rule should include tests covering:

* valid code
* invalid code
* automatic fixes
* nested blocks
* comments, including consecutive `//` blocks
* multiline `/* ... */` comments
* comment association across every supported control-flow construct
* preprocessor directives
* unusual whitespace
* LF and CRLF line endings
* repeated formatting/idempotency

## Building

### Restore Dependencies

Restore NuGet dependencies for the solution:

```console
dotnet restore CodeConform.slnx
```

This downloads any required NuGet packages that are not already available in the local NuGet cache.

Once the required dependencies have been restored, normal builds can operate without repeatedly downloading them.

### Build the Solution

Build the solution using the default Debug configuration:

```console
dotnet build CodeConform.slnx
```

This builds the analyzer, code-fix, validation, and test projects. CC0001–CC0004
are configured as errors in this repository, so a formatting violation in the
production analyzer or code-fix sources fails the solution build.

Because the test project uses the CodeConform analyzer during compilation, CodeConform diagnostics can also be reported while building the repository itself.

For example:

```text
CodeConform.CSharp.Tests\Analyzers\BlankLineBeforeReturnAnalyzerTests.cs(435,9):
error CC0001: A blank line is required before this return statement
```

### Run the Tests

Run all tests in the solution with:

```console
dotnet test CodeConform.slnx
```

The analyzer test suite verifies both correctly formatted source and source that is expected to produce CodeConform diagnostics.

## Building the NuGet Package

`CodeConform.CSharp` is distributed as one Roslyn analyzer package containing
the compiler-safe analyzer assembly and the separate code-fix assembly.

Run the following commands from the CodeConform repository root in PowerShell:

```powershell
dotnet restore .\CodeConform.slnx
dotnet test .\CodeConform.slnx -c Release --no-restore
dotnet pack `
    .\CodeConform.CSharp.Analyzers\CodeConform.CSharp.Analyzers.csproj `
    -c Release `
    --no-restore
```

The package is written to:

```text
CodeConform.CSharp.Analyzers/bin/Release/CodeConform.CSharp.0.1.0.nupkg
```

The filename uses the `<Version>` declared in
`CodeConform.CSharp.Analyzers/CodeConform.CSharp.Analyzers.csproj`.

For repeated local testing, use a unique prerelease version without editing the
project file:

```powershell
dotnet pack `
    .\CodeConform.CSharp.Analyzers\CodeConform.CSharp.Analyzers.csproj `
    -c Release `
    --no-restore `
    -p:Version=0.1.0-local.1
```

Increment the suffix for every rebuilt package, for example `local.2` and
`local.3`. NuGet caches packages by package ID and version, so overwriting a
package while retaining the same version can cause a consuming repository to
continue using cached contents.

### Analyzer Package Structure

`CodeConform.CSharp` is an analyzer package rather than a normal runtime library.

The project therefore configures the analyzer DLL to be stored inside the NuGet package under:

```text
analyzers/dotnet/cs/
```

The resulting package contains:

```text
analyzers/
└── dotnet/
    └── cs/
        ├── CodeConform.CSharp.Analyzers.dll
        └── CodeConform.CSharp.CodeFixes.dll
```

This location tells NuGet and Roslyn that the assembly should be loaded as a C# analyzer.

It is intentionally not packaged as:

```text
lib/netstandard2.0/CodeConform.CSharp.Analyzers.dll
```

because consuming applications do not need either assembly as a runtime
dependency.

## Using the NuGet Package Locally

A directory containing `.nupkg` files can be used as a local NuGet source. The
examples below use `D:\NuGetLocal`; another absolute directory can be used if
preferred.

### 1. Create and Register the Local Source

Create the directory once:

```powershell
New-Item -ItemType Directory -Path D:\NuGetLocal
```

Register it as a named source:

```powershell
dotnet nuget add source D:\NuGetLocal --name CodeConformLocal
```

Confirm that the source is registered and enabled:

```powershell
dotnet nuget list source
```

If the source already exists, `dotnet nuget add source` reports that fact and
does not need to be run again.

### 2. Pack Directly into the Local Source

From the CodeConform repository root, test and pack a unique local version
directly into the feed directory:

```powershell
dotnet test .\CodeConform.slnx -c Release

dotnet pack `
    .\CodeConform.CSharp.Analyzers\CodeConform.CSharp.Analyzers.csproj `
    -c Release `
    --no-restore `
    -p:Version=0.1.0-local.1 `
    --output D:\NuGetLocal
```

The resulting file is:

```text
D:\NuGetLocal\
└── CodeConform.CSharp.0.1.0-local.1.nupkg
```

### 3. Reference the Package from Another Repository

Copy `Scripts\Add-CodeConformToSolution.ps1` into the consuming repository's
`Scripts` directory. Change to the directory that contains the consuming
solution, then run the copied script:

```powershell
& .\Scripts\Add-CodeConformToSolution.ps1
```

The script discovers the single `.sln` or `.slnx` in the current directory and
uses the projects registered in that solution rather than every project found
below the directory. It selects the newest compatible CodeConform package,
including local prerelease versions, from `D:\NuGetLocal`. Existing references
are updated when necessary, and running the script again when every project is
current leaves the requested package versions unchanged. To use another local
feed, pass `-PackageSource` followed by its path.

The script identifies the CodeConform source solution by its registered
analyzer project, rather than by the location of the script. It therefore still
refuses to install CodeConform into CodeConform itself, while a copy under a
different solution works normally. The script also stops without making changes
if the current directory contains zero or multiple solution files.

For an analyzer-only dependency, the recommended project entry is:

```xml
<ItemGroup>
    <PackageReference Include="CodeConform.CSharp" Version="0.1.0-local.1">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
</ItemGroup>
```

`PrivateAssets="all"` prevents CodeConform from becoming a dependency of a
package produced by the consuming project. `IncludeAssets` retains analyzer
and build assets for the current project.

If the consuming repository uses Central Package Management, put the version
in its `Directory.Packages.props`:

```xml
<ItemGroup>
    <PackageVersion Include="CodeConform.CSharp" Version="0.1.0-local.1" />
</ItemGroup>
```

Then omit `Version` from the project reference:

```xml
<ItemGroup>
    <PackageReference Include="CodeConform.CSharp">
        <PrivateAssets>all</PrivateAssets>
        <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
</ItemGroup>
```

Restore and build the consuming solution from its directory:

```powershell
dotnet restore
dotnet build
```

### 4. Verify the Analyzer

Add deliberately non-conforming code inside a method in the consuming project:

```csharp
var result = GetResult();
return result;
```

Then build:

```console
dotnet build
```

CodeConform should report:

```text
CC0001: A blank line is required before this return statement
```

The severity is controlled by the consuming project's `.editorconfig`.

For example:

```ini
[*.cs]

dotnet_diagnostic.CC0001.severity = error
```

will cause the violation to be reported as an error and fail the build.

The correctly formatted source is:

```csharp
var result = GetResult();

return result;
```

To verify all current rules, examples of violations include:

```csharp
DoSomething();
if (condition)
{
}
DoSomethingElse();
// Explain the next operation.
DoAnotherThing();
```

These boundaries can produce CC0002, CC0003, and CC0004 respectively. In
Visual Studio, the corresponding code-fix provider should offer an action that
inserts the missing blank line. Reload the consuming solution after installing
or changing the analyzer package so the IDE loads the new assemblies.

### 5. Configure Diagnostic Severity

The consuming repository can configure each rule in `.editorconfig`:

```ini
[*.cs]

dotnet_diagnostic.CC0001.severity = warning
dotnet_diagnostic.CC0002.severity = warning
dotnet_diagnostic.CC0003.severity = warning
dotnet_diagnostic.CC0004.severity = warning
```

Use `error` while validating integration if the build should fail on a
violation.

### 6. Install a Rebuilt Local Version

Pack a new unique version in the CodeConform repository:

```powershell
dotnet pack `
    .\CodeConform.CSharp.Analyzers\CodeConform.CSharp.Analyzers.csproj `
    -c Release `
    -p:Version=0.1.0-local.2 `
    --output D:\NuGetLocal
```

Then update the consuming project or `Directory.Packages.props` from
`0.1.0-local.1` to `0.1.0-local.2` and restore again. Incrementing the version
is preferred to clearing all NuGet caches.

If an unchanged version must be reused during troubleshooting, clear NuGet's
local caches and restore the consuming solution:

```powershell
dotnet nuget locals all --clear
dotnet restore .\MyOtherSolution.slnx --force
```

Clearing all local caches affects every locally restored package, so use this
only when changing the package version is impractical.

## Package Development Workflow

A typical local development cycle is:

1. Implement and test a CodeConform change.
2. Run the Release test suite.
3. Pack a new `0.1.0-local.N` version into `D:\NuGetLocal`.
4. Update the version in the consuming repository.
5. Restore and build the consuming solution.
6. Open or reload the solution in Visual Studio and verify its code fixes.

## Status

`code-conform` is currently under initial development.

The current focus is C# formatting and linting using Roslyn. Additional languages or tooling may be considered in the future, but are not currently part of the project scope.

## License

Licensed under the Apache License 2.0. See `LICENSE` for details.
