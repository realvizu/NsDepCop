rd /S /Q out
msbuild.exe NsDepCop.sln /t:Rebuild /p:Configuration=Release
md out
xcopy NsDepCop.Setup\bin\Release\NsDepCop.msi out
