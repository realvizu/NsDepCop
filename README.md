# NsDepCop - Namespace and Assembly Dependency Checker Tool for C# #

NsDepCop is a static code analysis tool that enforces namespace and assembly dependency rules in C# projects.
* It runs as part of the build process and reports any dependency problems.
* No more unplanned or unnoticed dependencies in your system.

What is this [**dependency control**](doc/DependencyControl.md) anyway? And [**why should you automate it**](https://www.plainionist.net/Dependency-Governance-DotNet/)?

## Getting Started

1. Add the [**NsDepCop NuGet package**](https://nuget.org/packages/NsDepCop) to your C# projects: [![NuGet Package](https://img.shields.io/nuget/v/NsDepCop.svg)](https://nuget.org/packages/NsDepCop)
1. Add a text file named **'config.nsdepcop'** to your project, then edit it to define your [**dependency rules**](doc/Help.md#dependency-rules). 
1. Dependency violations will be underlined in the code editor and reported at build time just like compiler errors/warnings.

See the [**Help**](doc/Help.md) for details.

Or check out this step-by-step [**tutorial video**](https://www.youtube.com/watch?v=rkU7Hx20Dc0) by [plainionist](https://github.com/plainionist).

## Requirements

NsDepCop runs inside the C# compiler, so the **build toolchain** (Visual Studio / .NET SDK) determines compatibility, not the project's target framework:

| NsDepCop | Visual Studio | .NET SDK | Roslyn |
|---|---|---|---|
| 3.0+ | 2022 17.0+ | 6.0+ | 4.0+ |
| 2.7.x and earlier | 2019 16.9+ | 5.0+ | 3.9+ |

> Newer C# syntax is only analyzed when the toolchain's Roslyn supports it — e.g. C# 14 extension members need Roslyn 5+ (VS 2026 / .NET 10 SDK).

## Versions
* See the [**Change Log**](CHANGELOG.md) for version history.

## Feedback
* Use the [**Issue Tracker**](https://github.com/realvizu/NsDepCop/issues) to submit bugs and feature requests.
* Use the [**Discussions forum**](https://github.com/realvizu/NsDepCop/discussions) for questions.

## More Info
* [Diagnostics Reference](doc/Diagnostics.md)
* [Configuring XML schema support for config.nsdepcop files](doc/Help.md#config-xml-schema-support-in-visual-studio)
* [Troubleshooting](doc/Troubleshooting.md)
* [How to contribute?](Contribute.md)

## Thanks to 
* [Roslyn](https://github.com/dotnet/roslyn) for the amazing parser API.
* [DotNet.Glob](https://github.com/dazinator/DotNet.Glob) for the globbing library.

## License
* [GPL-2.0](LICENSE)

## Other Tools
* Check out my other project: [Codartis Diagram Tool](https://codartis.com/), a code visualization tool for C#.
