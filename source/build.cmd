rd /S /Q out
msbuild.exe NsDepCop.sln /t:Clean;Build /p:Configuration=Release
md out
xcopy NsDepCop.VisualStudioIntegration.Vsix\bin\Release\NsDepCop.vsix out
xcopy NsDepCop.Setup\bin\Release\NsDepCop.msi out
