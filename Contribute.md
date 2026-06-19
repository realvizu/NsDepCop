## How to build the source
1. Prerequisites
   * **Visual Studio 2022 17.0 or later** (any edition)
     * With workload: **Visual Studio extension development** (required for the VSIX project; it also installs a .NET SDK)
   * The **.NET 10 SDK** — used by CI and needed to build and test the C# 14 / Roslyn 5 analyzer variant
1. [Download or clone the source](https://github.com/realvizu/NsDepCop)
1. Open `source\NsDepCop.sln` and build it in Visual Studio.

   The analyzer dogfoods the `NsDepCop` NuGet package that this solution also produces, which creates a package self-reference cycle. Visual Studio resolves it internally, but a command-line **solution** restore/build does not. From the command line, build and test the **individual projects** instead — this is what the GitHub Actions CI does (see `.github/workflows/build.yml`):
   ```
   dotnet build source/NsDepCop.Analyzer/NsDepCop.Analyzer.csproj
   dotnet test source/NsDepCop.Test/NsDepCop.Test.csproj
   dotnet test source/NsDepCop.SourceTest/NsDepCop.SourceTest.csproj
   ```

## How to debug the tool in Visual Studio
1. Set **NsDepCop.Vsix** as the StartUp project.
1. In the project file modify the **StartArguments** tag to point to a valid solution file and log path.
1. Run the solution.
