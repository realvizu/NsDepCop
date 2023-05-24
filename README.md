# NsDepCop - Namespace Dependency Checker Tool for C# #

[![Build Status](https://ci.appveyor.com/api/projects/status/dm7q6tdwxv4xv85r?svg=true)](https://ci.appveyor.com/project/realvizu/nsdepcop)

NsDepCop is a static code analysis tool that helps you to enforce namespace dependency rules in C# projects.
* Runs as part of the build process and reports dependency problems.
* No more unplanned or unnoticed dependencies in your system.

What is this [**dependency control**](doc/DependencyControl.md) anyway? And [**why automate it**](https://www.plainionist.net/Dependency-Governance-DotNet/)?

## Getting Started

1. Add the **NsDepCop NuGet package** to your C# projects: [![NuGet Package](https://img.shields.io/nuget/v/NsDepCop.svg)](https://nuget.org/packages/NsDepCop)
1. Add a file called **config.nsdepcop**. Edit it and describe [**dependency rules**](doc/Help.md#dependency-rules). 
1. Dependency violations will be underlined in the code editor and also reported at build time just like compiler errors/warnings.

Or check out this step-by-step [**tutorial video**](https://www.youtube.com/watch?v=rkU7Hx20Dc0) by [plainionist](https://github.com/plainionist).

To get validation and IntelliSense while editing the config.nsdepcop files, add the config XML schema to the Visual Studio XML schema cache.
See details [here](doc/Help.md#config-xml-schema-support-in-visual-studio).

See the [**Help**](doc/Help.md) for details.

## Changes in v2.0

The big change in v2.0 is that the implementation changed from MSBuild task + Visual Studio Extension to a standard Roslyn analyzer.
- **Supports .NET Core / .NET 5+** projects too.
- **No need for the NsDepCop Visual Studio Extension** any more.
  - The NuGet package works both at build time and inside Visual Studio editor.
  - If the NuGet package is added to a project then it appears in Solution Explorer: project / Dependencies / Analyzers / NsDepCop.Analyzer
  - [Issue severities can be configured using Visual Studio light bulb menu or .editorconfig files](https://docs.microsoft.com/en-us/visualstudio/code-quality/use-roslyn-analyzers?view=vs-2019).
- **Requires Visual Studio 2019/2022 (16.10.0 or later).**
  - Dropped support for VS 2015/2017. For those, use NsDepCop v1.11.0.
- No need for the out-of-process service host any more.
  - No more "Unable to communicate with NsDepCop service".

Please note that the AutoLowerMaxIssueCount feature is temporarily not supported. Do not yet upgrade to v2.0 if you're using that.

## Versions
* See the [**Change Log**](CHANGELOG.md) for version history.

## Feedback
* Please use the [**Issue Tracker**](https://github.com/realvizu/NsDepCop/issues) to record bugs and feature requests.
* Or find me on twitter [![Follow on Titter](https://img.shields.io/twitter/url/http/realvizu.svg?style=social&label=@realvizu)](https://twitter.com/realvizu)

## More Info
* [Diagnostics Reference](doc/Diagnostics.md)
* [Troubleshooting](doc/Troubleshooting.md)
* [How to contribute?](Contribute.md)

## Thanks to 
* [Roslyn](https://github.com/dotnet/roslyn) for the best parser API.
* [ReSharper](https://www.jetbrains.com/resharper/) for the free licence of this amazing tool.
* [DotNet.Glob](https://github.com/dazinator/DotNet.Glob) for the globbing library.

## License
* [GPL-2.0](LICENSE)
