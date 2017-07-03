# NsDepCop - Namespace Dependency Checker Tool for C# #

[![Build Status](https://ci.appveyor.com/api/projects/status/dm7q6tdwxv4xv85r?svg=true)](https://ci.appveyor.com/project/realvizu/nsdepcop)

NsDepCop is a static code analysis tool that helps you to enforce namespace dependency rules in C# projects.
* Runs as part of the build process and reports dependency problems.
* No more unplanned or unnoticed dependencies in your system.

What is this [**dependency control**](doc/DependencyControl.md) anyway?

## Getting Started

1. Add the **NsDepCop NuGet package** to your C# projects.
   * Latest version, supports C# 7 language features: [![NuGet Package](https://img.shields.io/nuget/v/NsDepCop.svg)](https://nuget.org/packages/NsDepCop)
   * If you don't need C# 7 support then use the previous version which is faster: [![NuGet Package 1.6.1](https://img.shields.io/badge/nuget-1.6.1-blue.svg)](https://nuget.org/packages/NsDepCop/1.6.1)
1. A file called **config.nsdepcop** is automatically added to your project. Edit it and describe [**dependency rules**](doc/Help.md#dependency-rules).
   * If your project doesn't use the packages.config package manager format then you'll have to [add config.nsdepcop manually](doc/Troubleshooting.md#item4).
1. When you **build** the project, dependency violations will be reported in the build output just like compiler errors/warnings.

See the [**Help**](doc/Help.md) for details.

## Optional Stuff

* Install the **VSIX** (Visual Studio Extension) to get **instant** dependency check while editing the code.
  * For Visual Studio 2017: [![Visual Studio extension](https://img.shields.io/badge/Visual%20Studio%20Marketplace-NsDepCop%20VS2017-green.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCopVS2017-CodedependencycheckerforC)
  * For Visual Studio 2015: [![Visual Studio extension](https://img.shields.io/badge/Visual%20Studio%20Marketplace-NsDepCop%20VS2015-green.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCop-NamespacedependencycheckertoolforC)

* Install the **NsDepCop Config XML Schema Support** for Visual Studio to get validation and IntelliSense while editing the config.nsdepcop files.
  * For Visual Studio 2017: [![Visual Studio extension](https://img.shields.io/badge/Visual%20Studio%20Marketplace-NsDepCop%20Config%20XML%20Schema%20Support-green.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCopConfigXMLSchemaSupport)
  * For Visual Studio 2015: see the instructions in [Help](doc/Help.md##config-xml-schema-support-in-visual-studio).

## Versions
* See the [**Change Log**](CHANGELOG.md) for version history.
* See the [**Upgrade instructions**](CHANGELOG.md#upgrading) if upgrading from versions prior v1.6.0.
* See the [**Milestones**](https://github.com/realvizu/NsDepCop/milestones) for planned releases.
* See the [**Old Versions**](doc/Versions.md).

## Feedback
* Please use the [**Issue Tracker**](https://github.com/realvizu/NsDepCop/issues) to record bugs and feature requests.
* Or tweet me [![Follow on Titter](https://img.shields.io/twitter/url/http/realvizu.svg?style=social&label=@realvizu)](https://twitter.com/realvizu)

## More Info
* [Diagnostics Reference](doc/Diagnostics.md)
* [Troubleshooting](doc/Troubleshooting.md)
* [How to contribute?](Contribute.md)

## Thanks to 
* [Roslyn](https://github.com/dotnet/roslyn) for the best parser API.
* [ReSharper](https://www.jetbrains.com/resharper/) for the free licence of this amazing tool.

## License
* [GPL-2.0](LICENSE)
