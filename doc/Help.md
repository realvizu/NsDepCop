# NsDepCop Help

* [Supported project types](#supported-project-types)
* [Dependency rules](#dependency-rules)
* [Config inheritance](#config-inheritance)
* [Dealing with a high number of dependency issues](#dealing-with-a-high-number-of-dependency-issues)
* [Disabling with an environment variable](#disabling-with-an-environment-variable)
* [Config XML schema](#config-xml-schema)
* [Config XML schema support in Visual Studio](#config-xml-schema-support-in-visual-studio)
* [v1.x only topics](#v1.x-only-topics)

## Supported project types

* Projects with a **csproj** project file are supported.
* .Net Core and .Net 5+ projects are supported in v2.0 or above.
* Projects with an **xproj** project file are not supported.

## Dependency rules

* Allowed and disallowed dependencies are described with dependency rules in config files. 
* The rule config file must be named **config.nsdepcop** and its build action must be set to **C# analyzer additional file** (the NsDepCop NuGet package set it automatically).
* The default (and recommended) approach is [**allowlisting**](#allowlisting), that is, if a dependency is not explicitly allowed then it is disallowed. (See also: [denylisting](#denylisting)).
* The config file can inherit other config files from parent folders, see [**config inheritance**](#config-inheritance)
* Whe have two types of dependency checks: namespace and assembly dependency check
* The dependency check is disabled by default and needs to be enabled with **CheckAssemblyDependencies** attribute on the root element.


### Example One
```xml
<NsDepCopConfig IsEnabled="true" ChildCanDependOnParentImplicitly="true">
    <Allowed From="*" To="System.*" />
    <Allowed From="NsDepCop.*" To="Microsoft.CodeAnalysis.*" />
    <Allowed From="NsDepCop.ParserAdapter.Roslyn" To="NsDepCop.Analysis" />
</NsDepCopConfig>
```

Meaning:
* **Any** namespace can reference the **System** namespace and any of its sub-namespaces.
* The **NsDepCop** namespace and all of its sub-namespaces can reference the **Microsoft.CodeAnalysis** namespace and any of its sub-namespaces.
* The **NsDepCop.ParserAdapter.Roslyn** namespace can reference the **NsDepCop.Analysis** namespace (but not its sub-namespaces).

### Example Two
```xml
<NsDepCopConfig IsEnabled="true" CheckAssemblyDependencies="true">
    <AllowedAssembly From="*" To="*" />
    <DisallowedAssembly From="*.Repository" To="*.Service" />
</NsDepCopConfig>
```

Meaning:
* **Any** assembly can reference each other with one exception.
* The **Repository** layer cannot reference the **Service** layer.

### Config attributes
You can set the following attributes on the root element. (Bold marks the the **default** value.)

Attribute | Values | Description
--- | --- | ---
**IsEnabled** | **true**, false | If set to false then analysis is not performed for the project.
**ChildCanDependOnParentImplicitly** | true, **false** | If set to true then all child namespaces can depend on any of their parents without an explicit allowing rule. The recommended value is **true**. (False is default for backward compatibility.)
**ParentCanDependOnChildImplicitly** | true, **false** | If set to true then all parent namespaces can depend on any of their children without an explicit allowing rule. The recommended value is **false**.
**MaxIssueCount** | int (>0), default: **100** | Analysis stops when reaching this number of dependency issues.
**AutoLowerMaxIssueCount** | true, **false** | If set to true then each successful build yielding fewer issues than MaxIssueCount sets MaxIssueCount to the current number of issues.
**InheritanceDepth** | int (>=0), default: **0** | Sets the number of parent folder levels to inherit config from. 0 means no inheritance.
**ExcludedFiles** | Comma separated list of [file patterns](https://github.com/dazinator/DotNet.Glob) | Defines which source files should be excluded from the analysis. Paths are relative to the config file's folder. E.g.: `**/*.g.cs,TestFiles/*.cs`
**CheckAssemblyDependencies** | true, **false** | We adopt the 'disallowed-by-default' approach for assembly dependencies check, similar to how we handle namespace dependencies (where everything is disallowed unless explicitly permitted). To ensure the backward compatibility, this configuration attribute has been introduced to explicitly enable the assembly dependency checking. By default this attribute is false.

### Allowlisting
* The **`<Allowed From="N1" To="N2"/>`** config element defines that **N1** namespace can depend on **N2** namespace.
* If a dependency does not match any of the allowed rules then it's considered disallowed.

Special symbols:

Notation | Meaning
-- | --
. (a single dot) | The global namespace.
 \* (a single star) | Any namespace.
 MyNamespace.\* | MyNamespace and any sub-namespaces.
 MyNamespace.?| All direct sub-namespaces of MyNamespace
\*.MyNamespace | Any namespace named MyNamespace
?.MyNamespace | Any namespace named MyNamespace which has exactly one parent namespace.
MyNamespace.*.MyOtherNamespace| Any namespace called MyOtherNamespace which has an ancestor named MyNamespace
MyNamespace.?.MyOtherNamespace| Any namespace called MyOtherNamespace which has a grandparent named MyNamespace

Examples:

Example | Meaning
-|-
`<Allowed From="MyNamespace" To="System" />` | **MyNamespace** can depend on **System**
`<Allowed From="MyNamespace" To="System.*" />` | **MyNamespace** can depend on **System and any sub-namespace**
`<Allowed From="MyNamespace" To="*" />` | **MyNamespace** can depend on **any namespace**
`<Allowed From="MyNamespace" To="." />` | **MyNamespace** can depend on the **global namespace**
`<Allowed From="MyNamespace" To="System.*.Serialization.*" />` | **MyNamespace** can depend on all **Serialization** namespaces in **System** and their sub-namespaces

### Denylisting
* The **`<Disallowed From="N1" To="N2"/>`** config element defines that **N1** namespace **must not** depend on **N2** namespace.
* To implement the denylisting behavior, you also have to define an "allow all" rule, otherwise no dependency will be allowed.
* Only those dependencies are allowed that has a matching "Allowed" rule and no match with any of the "Disallowed" rules.
* You can specify any number of "Allowed" and "Disallowed" rules in any order.
* If both an "Allowed" and a "Disallowed" rule are matched then "Disallowed" is the "stronger".

Example:
```xml
<NsDepCopConfig>
    <Allowed From="*" To="*" />
    <Disallowed From="MyFrontEnd.*" To="MyDataAccess.*" />
</NsDepCopConfig>
```

Meaning:
* Every dependency is allowed but MyFrontEnd (and its sub-namespace) must not depend on MyDataAccess (and its sub-namespaces).

### Behavior of the wildcards '*' and '?'
If any `Disallowed` rule matches, no `Allowed` rule is considered.

If multiple `Allowed` rules match the same namespace, the one with best matching `From` rule is selected.

The best matching rule is the one with the minimal edit distance between namespace pattern and namespace name. The edit distance is calculated as the sum of all edit operations which are needed to replace the wildcards with the namespace names. The costs are as follows:
* Replacing a `?` has a cost of 1.
* Replacing a `*` has a cost of 1 and additionaly a cost of 1 per sub-namespace that replaces the `*`.

Example: When matching the namespace `A.B.C.D` the rule `A.?.?.D` (edit distance = 2) is preferred to the rule `A.*.D` (edit distance = 3). If multiple rules have the same edit distance, the behavior is undefined.

### Namespace surface
* The *surface* of a namespace consists of the types that are visible to some other namespace.
* The **`<VisibleMembers>`** config element defines the surface of a namespace.
* In the following example **GameLogic** can use only **Vector2** and **Vector3** types of the **UnityEngine** namespace.
```xml
<Allowed From="GameLogic" To="UnityEngine">
    <VisibleMembers>
        <Type Name="Vector2" />
        <Type Name="Vector3" />
    </VisibleMembers>
</Allowed>
```
* Notice that the surface is defined in the context of a particular namespace dependency, that is, this surface of **UnityEngine** is accessible only to **GameLogic**.
* You can define different surfaces for different other namespaces.
* You can also define a **"global"** surface, that is, a surface that is applicable to all namespaces that otherwise are allowed to depend on **UnityEngine**. See the following example. 
```xml
<VisibleMembers OfNamespace="UnityEngine">
    <Type Name="Vector2" />
    <Type Name="Vector3" />
</VisibleMembers>
```
* Notice that when defining a "global" surface the `<VisibleMembers>` element is not embedded in an `<Allowed>` element but you must specify the **OfNamespace** attribute.

### Allowing all child namespaces to depend on their parents
You can specify the **ChildCanDependOnParentImplicitly** attribute on the NsDepCopConfig element.
* True means that all child namespaces can depend on any of their parent namespaces without requiring an explicit Allowed rule.
* True is in line with how C# type resolution works: it searches parent namespaces without requiring an explicit using statement.
* False means that all dependencies between children and their parents must be explicitly allowed with a rule.
* False is the default for backward compatibility.

Example:
```xml
<NsDepCopConfig ChildCanDependOnParentImplicitly="true">
    <!-- The following rule is not necessary because the ChildCanDependOnParentImplicitly="true" attribute implies it. -->
    <Allowed From="MyNamespace.SubNamespace" To="MyNamespace" />
</NsDepCopConfig>
```

### Allowing all parent namespaces to depend on their children
You can specify the **ParentCanDependOnChildImplicitly** attribute on the NsDepCopConfig element.

However, this is **not recommended**, because child namespaces are usually more concrete/specialized than their parents and the dependecies should point from the more concrete/specialized to the more abstract/generic and not the other way.

## Config inheritance
From v1.6 NsDepCop supports config inheritance, aka multi-level config.

* The goal is to achieve "DRY" configs, that is, **avoid redundant info** in config.nsdepcop files.
* You can extract common config settings from project-level config.nsdepcop files and put them into a **"master"** config file, that must be in a folder that is a common ancestor of the project folders, e.g. the solution folder.
* The "master" config file must also be named config.nsdepcop.
* You have to **"turn on" inheritance** in the project-level configs by setting the `InheritanceDepth` attribute to a number that indicates the number of folder levels between the project folder and the master config's folder. 
* Typically you put the master config file into the solution folder which is the immediate parent of the project folders, so you set `InheritanceDepth="1"` in all project-level configs.

Example: 

```xml
config.nsdepcop file in "C:\MySolution":

<NsDepCopConfig ChildCanDependOnParentImplicitly="true">
    <Allowed From="*" To="System.*" />
    <Allowed From="*" To="MoreLinq" />
    <Allowed From="*" To="NsDepCop.Core.Util" />
</NsDepCopConfig>

config.nsdepcop file in "C:\MySolution\MyProject":

<NsDepCopConfig InheritanceDepth="1">
    <Allowed From="NsDepCop.MsBuildTask" To="NsDepCop.Core.Interface.*" />
    <Allowed From="NsDepCop.MsBuildTask" To="NsDepCop.Core.Factory" />
    <Allowed From="NsDepCop.MsBuildTask" To="Microsoft.Build.*" />
</NsDepCopConfig>
``` 

More info:
* If there is a conflict between the project-level and the inherited settings then the project-level settings "wins".
* The `IsEnabled` attribute has different meaning in the project-level config and in inherited configs.
  * If `IsEnabled="false"` in a project-level config then the project don't get analyzed.
  * If `IsEnabled="false"` in an inherited config then its content doesn't get inherited.
* The inheritance is not limited to just a project and a solution level config; you can have any number of config.nsdepcop files at any folder levels. Just make sure you set the `InheritanceDepth` to a number that is great enough to find all the higher-level configs.
* There must always be a config.nsdepcop file in the project folder if you want to analyze that project. 
Even if all the settings come from a higher-level config, you have to put **at least a minimal config to the project level**, that enables the inheritance in the first place.
E.g.: `<NsDepCopConfig InheritanceDepth="3"/>`

## Dealing with a high number of dependency issues
If there are so many dependency issues that you cannot fix them all at once but you still want to control them somehow then try the following.

* Prevent the introduction of more dependency issues. Set the current number of issues as the maximum and make it an error to create more.

```xml
<NsDepCopConfig MaxIssueCount="<the current number of issues>">
```

* Encourage developers to gradually fix the dependency issues by automatically lowering the max issue count whenever possible. Turn on AutoLowerMaxIssueCount.

```xml
<NsDepCopConfig AutoLowerMaxIssueCount="true" MaxIssueCount="<the current number of issues>">
```

> Please note that when NsDepCop modifies the nsdepcop.config files their formatting will be reset (because of the XML deserialization/serialization roundtrip).

## Disabling with an environment variable
To disable the tool **globally**, set the **DisableNsDepCop** environment variable to **true** or **1**.

`setx DisableNsDepCop 1`

It will affect both MSBuild integration (NuGet package) and Visual Studio integration (VSIX package).

Note that it won't affect processes that are already running, only the newly started ones.

## Config XML schema
See the XSD schema of config.nsdepcop [here](../source/NsDepCop.ConfigSchema/NsDepCopConfig.xsd).

## Config XML schema support in Visual Studio
Add NsDepCop config XML schema to the Visual Studio schema cache to get validation and IntelliSense when editing NsDepCop config files.

* Copy the following files into the Visual Studio schema cache folder, located at &lt;VsInstallDir&gt;/Xml/Schemas (eg. C:\Program Files\Microsoft Visual Studio\2022\Community\Xml\Schemas):
  * [NsDepCopCatalog.xml](../source/NsDepCop.ConfigSchema/NsDepCopCatalog.xml)
  * [NsDepCopConfig.xsd](../source/NsDepCop.ConfigSchema/NsDepCopConfig.xsd)

## v1.x only topics
The following topics apply only to v1.x versions.

### Config attributes deprecated in v2.0

Attribute | Values | Description
--- | --- | ---
**CodeIssueKind** | Info, **Warning**, Error | Dependency violations are reported at this severity level.
**InfoImportance** | Low, **Normal**, High | Info messages are reported to MSBuild at this level. This setting and the MSBuild verbosity (/v) swicth together determine whether a message appears on the output or not. See [Controlling verbosity](#controlling-verbosity) for details.
**MaxIssueCountSeverity** | Info, **Warning**, Error | This is the severity of the issue of reaching MaxIssueCount.
**AnalyzerServiceCallRetryTimeSpans** | Comma separated list of wait times in milliseconds, default: **100, 300, 1000, 3000, 10000** | These wait times are used between retries when the NsDepCop MsBuild Task cannot communicate with the out-of-process analyzer service.

### Controlling verbosity

* Besides emitting dependency violation issues, the tool can emit diagnostic and info messages too.
  * **Info messages** tell you when was the tool started and finished.
  * **Diagnostic messages** help you debug config problems by dumping config contents and dependency validation result cache change events.
* When the tool is run by MSBuild you can modify the [verbosity switch (/v:level)](https://msdn.microsoft.com/en-us/library/ms164311.aspx) to get more or less details in the output.
  * The verbosity levels defined by MSBuild are the following (ordered from less verbose to most verbose): q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic]
  * Set it to **detailed** or higher to see NsDepCop **diagnostic messages**.
  * Set it to **normal** or higher to see NsDepCop **info messages**.

Advanced settings:
* You can also modify the importance level of NsDepCop info messages by setting the `InfoImportance` attribute in config.nsdepcop to Low, Normal or High.
* This is useful if you want to keep the MSBuild verbosity level at a certain value for some reason (e.g.: because of other build steps you always want to keep verbosity at minimal level), and you want to control whether NsDepCop info messages are visible at that certain MSBuild verbosity level or not.
* The following table shows which InfoImportance levels are shown at certain MSBuild verbosity levels.

| MSBuild verbosity level| Low InfoImportance | Normal InfoImportance | High InfoImportance |
| - | - | - | - |
| q[uiet] | - | - | - |
| m[inimal] | - | - | yes |
| n[ormal] | - | yes | yes |
| d[etailed] | yes | yes | yes |
| diag[nostic] | yes | yes | yes |

E.g.: if you want NsDepCop info messages to show up at minimal MSBuild verbosity then set `InfoImportance` to High.

### Disabling to tool with MSBuild property
To disable the tool in MSBuild, set the **DisableNsDepCop** property to **true**.

```xml
<PropertyGroup>
  <DisableNsDepCop>true</DisableNsDepCop>
</PropertyGroup>
```

### Overriding the disabled state with MSBuild property
To force executing the tool in MSBuild, set the **ForceNsDepCop** property to **true**.

`msbuild MySolution.sln -t:NsDepCop_Analyze -p:ForceNsDepCop=true`

### Running NsDepCop only as an explicit command
If NsDepCop slows down the build too much then you can disable it as part of the build and run it explicitly before checking in.

* Disable NsDepCop in every build by creating a file called [Directory.Build.Props](https://docs.microsoft.com/en-us/visualstudio/msbuild/customize-your-build) in your source root directory with the following content:
 ```xml
<Project>
  <PropertyGroup>
    <DisableNsDepCop>true</DisableNsDepCop>
  </PropertyGroup>
</Project>
```

* Create a cmd file that runs only NsDepCop. Run it before every check-in.

`msbuild MySolution.sln -t:NsDepCop_Analyze -p:ForceNsDepCop=true`

### NsDepCop ServiceHost
NsDepCop NuGet package **v1.7.1** have introduced the NsDepCop ServiceHost to improve build performance.
* It runs in the background as a standalone process, communicates via named pipes and serves requests coming from NsDepCopTask instances running inside MSBuild processes.
* It is started automatically when needed by an NsDepCopTask and quits automatically when the MSBuild process that started it exits.
* By running continuously it avoids the repeated startup times which is significant.

You can control the lifetime of NsDepCop ServiceHost by controlling the lifetime of the MSBuild processes by modifying the **MSBUILDDISABLENODEREUSE** environment variable.
* If you set it to 1 then new MSBuild processes are started for each build and they exit when the build finishes. So do NsDepCop ServiceHost.
* If you set it to **0** then MSBuild processes are kept alive until the Visual Studio instance that started them exits. **This option gives the best build (and NsDepCop) performance.**
