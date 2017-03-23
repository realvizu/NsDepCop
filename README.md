# NsDepCop - Namespace Dependency Checker Tool for C# #

[![Build Status](https://ci.appveyor.com/api/projects/status/dm7q6tdwxv4xv85r?svg=true)](https://ci.appveyor.com/project/realvizu/nsdepcop)
[![NuGet Package](https://img.shields.io/nuget/v/NsDepCop.svg)](https://nuget.org/packages/NsDepCop)
[![Visual Studio extension](https://vsmarketplacebadge.apphb.com/version/FerencVizkeleti.NsDepCop-NamespacedependencycheckertoolforC.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCop-NamespacedependencycheckertoolforC)

NsDepCop is a static code analysis tool that lets you enforce namespace dependency rules in your C# projects.
* Runs as part of the build process and reports dependency problems.
* No more unplanned or unnoticed dependencies in your system.

What is this [**dependency control**](doc/DependencyControl.md) anyway?

## Getting Started

1. Add the [![NuGet Package](https://img.shields.io/nuget/v/NsDepCop.svg)](https://nuget.org/packages/NsDepCop) package to your C# projects.
1. A rule config file called config.nsdepcop is automatically added to the project.
1. Configure the dependency rules in the **config.nsdepcop** file.
1. When you **build** the project, dependency violations will be reported in the build output just like compiler errors/warnings.

See the [**Help**](doc/Help.md) for details.

## Optional Stuff

* Install the **VSIX** (Visual Studio Extension) to get **instant** dependency check while editing the code.
  * Get it from here: [![Visual Studio extension](https://vsmarketplacebadge.apphb.com/version/FerencVizkeleti.NsDepCop-NamespacedependencycheckertoolforC.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCop-NamespacedependencycheckertoolforC)
  * At the moment it is for Visual Studio 2015 only.

* Install the **config XML schema support** for Visual Studio to get validation and IntelliSense while editing the config.nsdepcop files.
  * Download the **MSI** installer from the [Releases](https://github.com/realvizu/nsdepcop/releases). 
  * Run the installer and choose the "Config XML schema support in VS" option.
  * Requires admin privilege for installing.
  * At the moment it is for Visual Studio 2015 only.
  
* There's a legacy option in the MSI installer called "Machine-wide MSBuild integration" but it's not recommended any more. Please use the NuGet package instead. (More info.)

## Versions
* See the [**Change Log**](CHANGELOG.md) for version history.
* See the [**Milestones**](https://github.com/realvizu/NsDepCop/milestones) for planned releases.
* See the [**Old Versions**](doc/Versions.md).

## Feedback
* Please use the [**Issue Tracker**](https://github.com/realvizu/NsDepCop/issues) to record bugs and feature requests.
* I post about new releases on Twitter: [![Follow on Titter](https://img.shields.io/twitter/url/http/realvizu.svg?style=social&label=@realvizu)](https://twitter.com/realvizu)

## More Info
* [Diagnostics Reference](doc/Diagnostics.md)
* [FAQ](doc/FAQ.md)

## Thanks to 
* [Roslyn](https://github.com/dotnet/roslyn) for the best parser API.
* [ReSharper](https://www.jetbrains.com/resharper/) for the free licence of this amazing tool.

## License
* [GPL-2.0](LICENSE)
