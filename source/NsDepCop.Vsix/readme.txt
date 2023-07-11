If the VSIX project launches the experimental Visual Studio instance successfully, but the breakpoints are not hit, 
then turn off "Run code analysis in separate process" in the Options of the experimental VS instance.
See: https://github.com/dotnet/roslyn-sdk/issues/515#issuecomment-1223731899

Also, if the project that is used for testing, has a package reference to the NsDepCop nuget, it might be necessary to remove it,
to force the analyzer to run from the VSIX package.
