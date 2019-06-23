# NsDepCop - Namespace Dependency Checker Tool for C# #

[![Build Status](https://ci.appveyor.com/api/projects/status/dm7q6tdwxv4xv85r?svg=true)](https://ci.appveyor.com/project/realvizu/nsdepcop)

NsDepCop is a static code analysis tool that helps you to enforce namespace dependency rules in C# projects.
* Runs as part of the build process and reports dependency problems.
* No more unplanned or unnoticed dependencies in your system.

What is this [**dependency control**](doc/DependencyControl.md) anyway?

## Getting Started

1. Add the **NsDepCop NuGet package** to your C# projects: [![NuGet Package](https://img.shields.io/nuget/v/NsDepCop.svg)](https://nuget.org/packages/NsDepCop)
1. Add a file called **config.nsdepcop**. Edit it and describe [**dependency rules**](doc/Help.md#dependency-rules). 
   * For projects that use the old packages.config nuget format the file is automatically added.
1. When you **build** the project, dependency violations will be reported in the build output just like compiler errors/warnings.

See the [**Help**](doc/Help.md) for details.

## Optional Stuff

* Install the **VSIX** (Visual Studio Extension) to get **instant** dependency check while editing the code.
  * For Visual Studio 2017/2019: [![Visual Studio extension](https://img.shields.io/badge/Visual%20Studio%20Marketplace-NsDepCop-green.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCopVS2017-CodedependencycheckerforC)
  * For Visual Studio 2015: [![Visual Studio extension](https://img.shields.io/badge/Visual%20Studio%20Marketplace-NsDepCop%20VS2015-green.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCop-NamespacedependencycheckertoolforC)

* Install the **NsDepCop Config XML Schema Support** for Visual Studio to get validation and IntelliSense while editing the config.nsdepcop files.
  * For Visual Studio 2017/2019: [![Visual Studio extension](https://img.shields.io/badge/Visual%20Studio%20Marketplace-NsDepCop%20Config%20XML%20Schema%20Support-green.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCopConfigXMLSchemaSupport)
  * For Visual Studio 2015: see the instructions in [Help](doc/Help.md#config-xml-schema-support-in-visual-studio).

## Versions
* See the [**Change Log**](CHANGELOG.md) for version history.
* See the [**Upgrade instructions**](CHANGELOG.md#upgrading) if upgrading from versions prior to v1.6.0.
* See the [**Milestones**](https://github.com/realvizu/NsDepCop/milestones) for planned releases.

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
