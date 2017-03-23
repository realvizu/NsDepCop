# NsDepCop Help

* [Dependency rules](#dependency-rules)
* [Config inheritance](#config-inheritance)
* [Controlling verbosity](#controlling-verbosity)
* [Config.nsdepcop schema](#config.nsdepcop-schema)
* [Machine-wide MSBuild integration](#machine-wide-msbuild-integration)


## Dependency rules

* Allowed and disallowed dependencies are described with dependency rules in config files. 
* The rule config file must be named **config.nsdepcop** and must be placed next to the csproj file.
* The default (and recommended) approach is [**whitelisting**](#whitelisting), that is, if a dependency is not explicitly allowed then it is disallowed. (See also: [blacklisting](#blacklisting)).
* The config file can inherit other config files from parent folders, see [**config inheritance**](#config-inheritance)

### Example
```xml
<NsDepCopConfig IsEnabled="true" CodeIssueKind="Warning" ChildCanDependOnParentImplicitly="true">
    <Allowed From="*" To="System.*" />
    <Allowed From="NsDepCop.MsBuildTask" To="NsDepCop.Core" />
    <Allowed From="NsDepCop.MsBuildTask" To="Microsoft.Build.*" />
</NsDepCopConfig>
```

Meaning:
* **Any** namespace can reference the **System** namespace and any of its sub-namespaces.
* The **NsDepCop.MsBuildTask** namespace can reference the **NsDepCop.Core** namespace (but not its sub-namespaces).
* The **NsDepCop.MsBuildTask** namespace can reference the **Microsoft.Build** namespace and its sub-namespaces.

### Config attributes
You can set the following attributes on the root element. (Bold marks the the **default** value.)

Attribute | Values | Description
--- | --- | ---
**IsEnabled** | **true**, false | If set to false then analysis is not performed for the project.
**CodeIssueKind** | Info, **Warning**, Error | Dependency violations are reported at this severity level.
**ChildCanDependOnParentImplicitly** | true, **false** | If set to true then all child namespaces can depend on any of their parents without an explicit allowing rule. The recommended value is **true**. (False is default for backward compatibility.)
**InfoImportance** | Low, **Normal**, High | Info messages are reported to MSBuild at this level. This setting and the MSBuild verbosity (-v) swicth together determine whether a message appears on the output or not. See [InfoImportance](#controlling-verbosity) for details.
**MaxIssueCount** | int (>0), default: **100** | Analysis stop when reaching this number of issues.
**InheritanceDepth** | int (>=0), default: **0** | Sets the number of parent folder to inherit config from. 0 means no inheritance.

### Whitelisting
* The **`<Allowed From="N1" To="N2"/>`** config element defines that **N1** namespace can depend on **N2** namespace.
* If a dependency does not match any allowed rule then it's considered disallowed.

Special symbols:

Notation | Meaning
-- | --
. (a single dot) | The global namespace.
 \* (a single star) | Any namespace.
 MyNamespace.\* | MyNamespace and any sub-namespaces.

Examples:

Example | Meaning
-|-
`<Allowed From="MyNamespace" To="System" />` | **MyNamespace** can depend on **System**
`<Allowed From="MyNamespace" To="System.*" />` | **MyNamespace** can depend on **System and any sub-namespace**
`<Allowed From="MyNamespace" To="*" />` | **MyNamespace** can depend on **any namespace**
`<Allowed From="MyNamespace" To="." />` | **MyNamespace** can depend on the **global namespace**

### Blacklisting
* The **`<Disallowed From="N1" To="N2"/>`** config element defines that **N1** namespace **must not** depend on **N2** namespace.
* To implement the blacklisting behavior, you also have to define an "allow all" rule, otherwise no dependency will be allowed.
* Only those dependencies are allowed that has a matching "Allowed" rule and no match with any of the "Disallowed" rules.
* You can specify any number of "Allowed" and "Disallowed" rules in any order.
* If both an "Allowed" and a "Disallowed" rule is matched then "Disallowed" is the "stronger".

Example:
```xml
<NsDepCopConfig>
    <Allowed From="*" To="*" />
    <Disallowed From="MyFrontEnd.*" To="MyDataAccess.*" />
</NsDepCopConfig>
```

Meaning:
* Every dependency is allowed but MyFrontEnd (and its sub-namespace) must not depend on MyDataAccess (and its sub-namespaces).

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
* Other namespaces still can't depend on **UnityEngine**.
* You can define different surfaces for different other namespaces.
* You can also define a **"global"** surface, that is, a surface that is applicable to all namespaces that otherwise are allowed to depend on **UnityEngine**. See the following example. 
```xml
<VisibleMembers OfNamespace="UnityEngine">
    <Type Name="Vector2" />
    <Type Name="Vector3" />
</VisibleMembers>
```
* Notice that when defining a "global" surface the `<VisibleMembers>` element is not embedded in an `<Allowed>` element but use must specify the **OfNamespace** attribute.

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

## Config inheritance
aka Multi-level config

## Controlling verbosity

## Config.nsdepcop schema
See the XSD schema of config.nsdepcop [here](../source/NsDepCop.Setup/NsDepCopConfigSchema/NsDepCopConfig.xsd).

## Machine-wide MSBuild integration
* This is a legacy option in the MSI installer and requires admin privilege.
* Hooks into the MSBuild C# build process by modifying the "Custom.After.Microsoft.CSharp.targets" file. It does not modify any csproj files.
* Runs NsDepCop when building any C# project that has a config.nsdepcop file.
* The drawback of this method is that you have to install the tool on every environment where you want to use it.
* The NuGet (per-project MSBuild integration) approach is much better because that works in every environment with zero install: the tool gets pulled down by the nuget package restore.
 