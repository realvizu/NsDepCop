# NsDepCop Help

## Dependency Rules

TBW

## Machine-wide MSBuild integration
* This is a legacy option in the MSI installer and requires admin privilege.
* Hooks into the MSBuild C# build process by modifying the "Custom.After.Microsoft.CSharp.targets" file. It does not modify any csproj files.
* Runs NsDepCop when building any C# project that has a config.nsdepcop file.
* The drawback of this method is that you have to install the tool on every environment where you want to use it.
* The NuGet (per-project MSBuild integration) approach is much better because that works in every environment with zero install: the tool gets pulled down by the nuget package restore.
 