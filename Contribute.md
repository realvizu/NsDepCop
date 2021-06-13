## How to build the source
1. Prerequisites
   * Visual Studio 2019 (16.10.0 or later, any edition)
     * With workload: **Visual Studio extension development**
1. [Download or clone the source](https://github.com/realvizu/NsDepCop)
1. Open "source\NsDepCop.sln"
1. Build the solution.

## How to debug the tool in Visual Studio
1. Set **NsDepCop.Vsix** as the StartUp project.
1. In the project file modify the **StartArguments** tag to point to a valid solution file and log path.
1. Run the solution.