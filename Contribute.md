# How to Contribute to NsDepCop

## How to build the source
1. Prerequisites
   * Visual Studio 2017 (any edition)
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
