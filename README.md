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

## Current Scope

The initial implementation supports **C# only** and is built using the .NET Compiler Platform (Roslyn).

The first set of rules focuses on whitespace and blank-line conventions that are not provided by `dotnet format`.

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

The initial repository is intentionally small:

```text
code-conform/
├── src/
│   ├── CodeConform.CSharp/
│   └── CodeConform.Tool/
│
├── tests/
│   └── CodeConform.CSharp.Tests/
│
├── CodeConform.slnx
├── Directory.Build.props
├── Directory.Packages.props
├── LICENSE
└── README.md
```

### CodeConform.CSharp

Contains the Roslyn-based implementation:

* diagnostic analyzers
* code fix providers
* formatting rules
* syntax and trivia helpers

The same rule implementation should be used by IDE analysis and command-line tooling to ensure consistent behaviour.

### CodeConform.CSharp.Tests

Contains analyzer and code-fix tests.

Formatting behaviour should be thoroughly tested against normal and unusual C# syntax before rules are considered stable.

### CodeConform.Tool

Command-line integration for repository-wide checking and fixing.

The intended command is:

```console
dotnet conform
```

Additional commands may include:

```console
dotnet conform check
dotnet conform fix
```

The command-line tool is secondary to the Roslyn analyzer implementation and may not be required during the initial development phase.

## Visual Studio Integration

`CodeConform.CSharp` is intended to be distributed as a NuGet analyzer package.

Projects can reference the analyzer package and receive diagnostics directly while editing C# code in Visual Studio.

For example:

```csharp
DoSomething();
return result;
```

may produce a diagnostic such as:

```text
CC0001: A blank line is required before this return statement.
```

with an associated code fix producing:

```csharp
DoSomething();

return result;
```

## Configuration

Rule severity should use the standard Roslyn `.editorconfig` mechanism where possible.

For example:

```ini
[*.cs]

dotnet_diagnostic.CC0001.severity = warning
dotnet_diagnostic.CC0002.severity = warning
```

This allows repositories to enable, disable, or change the severity of individual rules without introducing a separate configuration system.

## `dotnet format`

`code-conform` is intended to work alongside `dotnet format`, not replace it.

A typical repository formatting workflow may eventually be:

```console
dotnet format
dotnet conform
```

Where possible, CodeConform analyzers and code fixes may also be executable through the analyzer support provided by `dotnet format`.

## Rule IDs

CodeConform diagnostics use the `CC` prefix.

For example:

| Rule     | Description                                       |
| -------- | ------------------------------------------------- |
| `CC0001` | Blank line required before control-flow statement |
| `CC0002` | Blank line required before `return` statement     |
| `CC0003` | Blank line required after closing brace           |
| `CC0004` | Blank line required before comment block          |

Rule IDs and definitions may change while the project is under initial development.

## Design Principles

### Deterministic

Running a formatter repeatedly should not continue changing the source:

```text
Conform(Conform(source)) == Conform(source)
```

### Syntax-aware

C# source is analysed using Roslyn syntax trees rather than regular expressions or line-oriented source manipulation.

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
* comments
* multiline comments
* control-flow constructs
* preprocessor directives
* unusual whitespace
* LF and CRLF line endings
* repeated formatting/idempotency

## Building

Restore and build the solution using the .NET SDK:

```console
dotnet restore
dotnet build
```

Run the tests with:

```console
dotnet test
```

Create NuGet packages with:

```console
dotnet pack
```

## Status

`code-conform` is currently under initial development.

The current focus is C# formatting and linting using Roslyn. Additional languages or tooling may be considered in the future, but are not currently part of the project scope.

## License

Licensed under the Apache License 2.0. See `LICENSE` for details.
