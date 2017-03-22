# How to Contribute to NsDepCop

## How to build the source
1. Prerequisites
   * Visual Studio 2015 Update 3 (any edition)
   * [Visual Studio 2015 SDK Update 3](https://msdn.microsoft.com/en-us/library/mt683786.aspx)
   * [WiX Toolset v3.10](http://wixtoolset.org/releases/v3-10-0-1823/)
1. [Download or clone the source](https://github.com/realvizu/NsDepCop)
1. Open "source\NsDepCop.sln"
1. Enable NuGet Package restore in Visual Studio.
1. Build the solution.

## How to debug the tool in Visual Studio
1. Set **NsDepCop.VisualStudioIntegration.Vsix** as the StartUp project.
1. In the project properties set up the debug start action.
   * **Start external program**: C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\devenv.exe
   * **Command line arguments**: /rootsuffix Roslyn 
1. Run the solution.
