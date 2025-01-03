# NsDepCop - Namespace and Assembly Dependency Checker Tool for C# #

[![Build Status](https://ci.appveyor.com/api/projects/status/dm7q6tdwxv4xv85r?svg=true)](https://ci.appveyor.com/project/realvizu/nsdepcop)

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
