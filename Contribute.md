# How to Contribute to NsDepCop
  * [How to build the source](#how-to-build-the-source)
  * [How to debug the tool in Visual Studio 2017](#how-to-debug-the-tool-in-visual-studio-2017)
  * [How to debug the tool in Visual Studio 2015](#how-to-debug-the-tool-in-visual-studio-2015)
  * [Why have different VSIX for VS2015 and VS2017?](#why-have-different-vsix-for-vs2015-and-vs2017)

## How to build the source
1. Prerequisites
   * Visual Studio 2017/2019 (any edition)
     * With workload: **Visual Studio extension development**
1. [Download or clone the source](https://github.com/realvizu/NsDepCop)
1. Open "source\NsDepCop.sln"
1. Build the solution.

## How to debug the tool in Visual Studio 2017
1. Set **NsDepCop.VisualStudioIntegration.Vs2017** as the StartUp project.
1. In the project properties set up the debug start action.
   * **Start external program**: the path to VS2017 devenv.exe
     * E.g.: C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe
   * **Command line arguments**: /rootsuffix Exp 
1. Run the solution.

## How to debug the tool in Visual Studio 2015
1. Set **NsDepCop.VisualStudioIntegration.Vs2015** as the StartUp project.
1. In the project properties set up the debug start action.
   * **Start external program**: the path to VS2015 devenv.exe
     * E.g.: C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe
   * **Command line arguments**: /rootsuffix Roslyn 
1. Run the solution.

## Why have different VSIX for VS2015 and VS2017?
Because they are built with different Roslyn version.
* VS2015 uses Roslyn 1.x.x (C# 6.0)
* VS2017 uses Roslyn 2.x.x (C# 7.x)

See:
* https://docs.microsoft.com/en-us/visualstudio/extensibility/roslyn-version-support
* https://github.com/dotnet/roslyn/wiki/NuGet-packages
