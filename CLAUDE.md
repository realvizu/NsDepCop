# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

NsDepCop is a Roslyn-based static analysis tool for C# that enforces namespace and assembly dependency rules. It ships as a NuGet package that integrates into the build process, reporting violations as compiler warnings/errors.

## Build and Test Commands

Solution-level `dotnet build/test` fails due to a self-referencing NuGet cycle (`NsDepCop.Analyzer` depends on the `NsDepCop` NuGet package, which is produced by `NsDepCop.NuGet` in the same solution). CI avoids this with standalone `nuget.exe restore` + Framework `msbuild`; VS IDE handles it internally. From the command line, target individual projects:

```bash
# Build the analyzer (restores and builds dependencies automatically)
dotnet build source/NsDepCop.Analyzer/NsDepCop.Analyzer.csproj

# Run unit tests
dotnet test source/NsDepCop.Test/NsDepCop.Test.csproj

# Run source-based integration tests
dotnet test source/NsDepCop.SourceTest/NsDepCop.SourceTest.csproj

# Run a single test by name
dotnet test source/NsDepCop.Test/NsDepCop.Test.csproj --filter "FullyQualifiedName~TestMethodName"
```

**Prerequisites:** Visual Studio 2022 with **Visual Studio extension development** workload (includes the .NET SDK).

## Solution Structure

All source code lives under `source/`. The solution is `source/NsDepCop.sln`.

| Project | Target | Purpose |
|---|---|---|
| `NsDepCop.Analyzer` | netstandard2.0 | Core analyzer — the main product code |
| `NsDepCop.Test` | net8.0 | Unit tests (xUnit, FluentAssertions, Moq) |
| `NsDepCop.SourceTest` | net8.0 | Integration tests — verifies analyzer against C# source files |
| `NsDepCop.NuGet` | netstandard2.0 | NuGet package packaging |
| `NsDepCop.Benchmarks` | — | Performance benchmarks |
| `NsDepCop.Vsix` | — | Visual Studio Extension wrapper |

## Architecture

### Analyzer Core (`NsDepCop.Analyzer`)

Root namespace: `Codartis.NsDepCop`. Internal module dependencies are enforced by `config.nsdepcop` in the project itself.

- **RoslynAnalyzer/** — Entry point. `NsDepCopAnalyzer` extends Roslyn `DiagnosticAnalyzer`, registered for `IdentifierName`, `GenericName`, and `DefaultLiteralExpression` syntax kinds. Wires together config and analysis. Must only depend on other modules via interfaces (not `.Implementation` namespaces).
- **Config/** — XML config file parsing and rule model. `DependencyRule`, `Domain`, `WildcardDomain`, `RegexDomain` represent rules. `MultiLevelXmlFileConfigProvider` handles config inheritance (project → parent directories). Factory pattern separates creation from implementation.
- **Analysis/** — Dependency validation logic. `DependencyAnalyzer` orchestrates type-level and assembly-level validation. `TypeDependencyValidator` checks namespace rules; `AssemblyDependencyValidator` checks assembly rules.
- **ParserAdapter/Roslyn/** — Extracts type dependencies from Roslyn syntax trees.
- **Util/** — Shared helpers.

### Dependency flow between modules

```
RoslynAnalyzer → Config (interfaces only), Analysis (interfaces only), ParserAdapter
ParserAdapter  → Analysis
Analysis       → Config
Config.Factory → Config.Implementation
```

The `RoslynAnalyzer` layer is explicitly **disallowed** from depending on `*.Implementation` namespaces.

### Test Structure

**Unit tests** (`NsDepCop.Test`): Standard xUnit tests for config parsing, validation logic, and analyzer behavior. Test data files (`.nsdepcop` configs) are in subdirectories named after their test class, copied to output via `CopyToOutputDirectory`.

**Source tests** (`NsDepCop.SourceTest`): Each test case is a folder containing a `.cs` source file and a `config.nsdepcop` file. The `.cs` files are excluded from compilation (`<Compile Remove>`) and instead copied to output as test data. Tests verify the analyzer produces expected diagnostics for various C# syntax patterns (C# 6, 7, 7.1, 7.2, 7.3, top-level statements).

### Self-referencing / Dogfooding

The project references its own NuGet package (`NsDepCop 2.7.0`) and enforces dependency rules on its own code. `Directory.Build.targets` contains a workaround (`AvoidCycleErrorOnSelfReference`) that renames `PackageId` to `NsDepCop_temp` during build to break the cycle, restoring it before pack. This workaround is broken with current .NET SDK versions — `dotnet restore` and `msbuild /t:Restore` still detect the cycle. Only standalone `nuget.exe restore` (used by CI) and VS IDE's internal restore avoid the error. This is why command-line builds must target individual projects rather than the solution.

## Key Conventions

- `TreatWarningsAsErrors` is enabled on all projects
- Root namespace is `Codartis.NsDepCop` (with project-specific suffixes like `.Test`)
- `config.nsdepcop` is the XML configuration file format — both the product config and test fixtures
- CI runs on GitHub Actions (`.github/workflows/build.yml`, `ubuntu-latest`), Release configuration, using per-project `dotnet test`/`dotnet pack` (not a solution build, to avoid the self-reference cycle)
- Version is managed via MSBuild properties: `VersionPrefix` in `source/Directory.Build.props` is the single source of truth (with shared `Company`/`Product`/`Copyright`); CI sets a `VersionSuffix` prerelease tag for non-release builds. `AssemblyVersion` is pinned stable per major.

## Diagnostics

Diagnostic IDs: `NSDEPCOP01` (illegal namespace dependency), `NSDEPCOP02` (too many issues), `NSDEPCOP03` (no config), `NSDEPCOP04` (config disabled), `NSDEPCOP05` (config error), `NSDEPCOP06` (tool disabled), `NSDEPCOP07` (illegal assembly dependency). Definitions are in `DiagnosticDefinitions.cs`.
