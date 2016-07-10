param (
    [Parameter(Mandatory=$true)][string]$version
)

[string]$crLf = "`r`n"

function Update-VersionInfoCs([string]$filename)
{
	[string]$fileContent = "using System.Reflection;$crLf" 
	$fileContent += $crLf
	$fileContent += "[assembly: AssemblyVersion(""$version"")]$crLf"
	$fileContent += "[assembly: AssemblyFileVersion(""$version"")]"
	$fileContent | Out-File $filename
}

function Update-VsixManifest([string]$filename)
{
	[xml]$xmlContent = Get-Content $filename
	$xmlContent.PackageManifest.Metadata.Identity.Version = $version
	$xmlContent.Save($filename)
}

Update-VersionInfoCs "include\VersionInfo.cs"
Update-VsixManifest "NsDepCop.VisualStudioIntegration.Vsix\source.extension.vsixmanifest"
