# NsDepCop FAQ

1. [Anonymous types raise false alarms](#faq01)
1. [Setup is unable to add NsDepCop target to the C# build workflow](#faq02)
1. [Setup is unable to remove NsDepCop target from the C# build workflow](#faq03)
1. [Where can I download the .NET Compiler Platform (Roslyn) End User Preview (for Visual Studio 2013)?](#faq04)

## FAQ01
### Anonymous types raise false alarms

> Applies to versions before v1.6 only. NsDepCop v1.6 leaves anonymous types out of the analysis. 

For anonymous types the compiler generates a class with no namespace so they will belong to the 'global namespace'. If your NsDepCop config does not allow referencing the global namespace (denoted with a single dot) then it will raise an alarm that you may consider a false positive.

To avoid alarms caused by anonymous types you have to add a rule that you allow referencing the global namespace:
```xml
<Allowed From="YourNamespace" To="." />
```

Or to be more lax:
```xml
<Allowed From="*" To="." />
```

## FAQ02
### Setup is unable to add NsDepCop target to the C# build workflow

In order to insert the NsDepCop task into the C# project build workflow, setup creates (or modifies) the "**Custom.After.Microsoft.CSharp.targets**" file at "(Program Files folder)\MsBuild\v14.0".
It redefines the "BuildDependsOn" property group to include the NsDepCop target as the last step just before the "Build" target executes. 
If you already have this file and it already contains the definition of the BuildDependsOn property group then setup won't change it (so not to mess up anything). 

In this case you have to insert the NsDepCop target manually. 
Append a semicolon and the string "NsDepCop" at the end of the text inside the "BuildDependsOn" tags.

Before:
```xml
<PropertyGroup>
  <BuildDependsOn>
    $(BuildDependsOn);
    MyCustomTask
  </BuildDependsOn>
</PropertyGroup>"
```
After:
```xml
<PropertyGroup>
  <BuildDependsOn>
    $(BuildDependsOn);
    MyCustomTask;
    NsDepCop
  </BuildDependsOn>
</PropertyGroup>
```

## FAQ03
### Setup is unable to remove NsDepCop target from the C# build workflow

The same problem as above when uninstalling NsDepCop. 
The "**Custom.After.Microsoft.CSharp.targets**" file at "(Program Files folder)\MsBuild\v14.0" contains a definition of the BuildDependsOn property group that is different from the content that setup creates. 
You have to remove the NsDepCop string from the BuildDependsOn definition manually.

## FAQ04
### Where can I download the .NET Compiler Platform (Roslyn) End User Preview (for Visual Studio 2013)?

Unfortunately .NET Compiler Platform (Roslyn) End User Preview (for Visual Studio 2013) is not available from Microsoft to download any more.
You can find it in the source tree: [VS2013\prereq\Roslyn End User Preview.vsix](https://github.com/realvizu/NsDepCop/blob/VS2013/prereq/Roslyn%20End%20User%20Preview.vsix)
