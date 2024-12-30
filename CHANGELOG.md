# NsDepCop Change Log

## v2.5.0
(12/2024)

- [x] New: Implement a new code analysis rule to check illegal assembly references.

## v2.4.0
(09/2024)

- [x] New: Analyzing top level statements.

## v2.3.0
(07/2023)

- [x] Changed: Added the allowed type names to the diagnostic message (if VisibleMembers are specified).

## v2.2.0
(09/2022)

- [x] New: Wildcard patterns for namespaces.

## v2.1.0
(07/2022)

- [x] New: ParentCanDependOnChildImplicitly config attribute.

## v2.0.1
(03/2022)

- [x] Fix: #60 - Statically imported type dependency not detected.

## v2.0.0
(06/2021)

The big change in this version is that the implementation changed from MSBuild task + Visual Studio Extension to a standard Roslyn analyzer.

  - [x] NsDepCop must be added to a project as a NuGet package.
  - [x] Appears in Solution Explorer: project / Dependencies / Analyzers / NsDepCop.Analyzer
  - [x] Issue severities can be configured that same way as other analyzers (use Visual Studio or .editorconfig files).
  - [x] Works both at build time and inside Visual Studio editor.
  - [x] Requires Visual Studio 2019 (16.10.0 or later).
  - [x] Supports .NET Core / .NET 5 / etc. projects too.
  - [x] Uses Roslyn 3.9.0.

Stuff that was removed:
  - [x] No need for the NsDepCop Visual Studio Extension any more.
  - [x] No need for the out-of-process service host any more.
  - [x] Dropped support for VS 2015/2017. For those, use NsDepCop v1.11.0.
  - [x] Config attributes no longer supported (ignored): CodeIssueKind, MaxIssueCountSeverity, InfoImportance, AnalyzerServiceCallRetryTimeSpans. 

Other info:
  - [x] AutoLowerMaxIssueCount feature is temporarily not supported.

## v1.11.0
(04/2020)

- [x] New: Disable/force NsDepCop with MSBuild property. 

## v1.10.1
(02/2020)

- [x] Fix: #51 - RemotingException when using different NsDepCop NuGet and VSIX version.

## v1.10.0
(04/2019)

- [x] New: Support Visual Studio 2019

## v1.9.0
(03/2019)

- [x] New: Support incremental build - don't run the tool if there was no change in the source/config files.
- [x] New: Global turn off switch - DisableNsDepCop environment variable.
- [x] New: Excluding files from analysis - ExcludedFiles attribute in config.nsdepcop.

## v1.8.2
(01/2019)

- [x] Fix: #43 (for real this time) - RemotingException when path contains space.

## v1.8.1
(12/2018)

- [x] Fix: #43 - RemotingException when path contains space.

## v1.8.0
(07/2018)

- [x] Enhancements in launching the out-of-process service host (configurable retry intervals, allow access for any user).
- [x] MaxIssueCountSeverity - enables breaking the build when a threshold number of dependency violations has been reached.
- [x] AutoLowerMaxIssueCount - automatically lower MaxIssueCount to encourage cleaning up dependency problems and prohibit introducing new ones.
- [x] Supports C# up to 7.3.
- [x] The Visual Studio Extension (VSIX) requires 15.7.4 or higher.

## v1.7.1
(07/2017)

- [x] Performance enhancement when run by MSBuild. 
  - [x] The analyzer keeps running in its own process to avoid repeated creation cost. Shuts down with parent process.

## v1.7.0
(07/2017)

- [x] New: C# 7 support.
- [x] New: Visual Studio 2017 support.
- [x] Removed: MSI installer.
  - [x] Config XML schema support is now available as a Visual Studio Extension (only for VS2017).
  - [x] Global MSBuild-integration is discontinued. Please use per-project MSBuild integration via the NuGet package.

## v1.6.1
(04/2017)

- [x] Fix: Type without a name caused exception in analyzer.

## v1.6.0
(03/2017)

- [x] New: NuGet package.
  - [x] Enables per-project MSBuild integration.
  - [x] Enables zero install on build machine.
- [x] New: Multi-level config file.
  - [x] Use the InheritanceDepth config attribute to specify the number of parent folder levels to merge config files from.
  - [x] Use it to get rid of redundant rules and settings in nsdepcop.config files and move them to a common place, eg. to solution level or repo root level.
- [x] Changed: MSI installer modified.
  - [x] Option (default on): config.nsdepcop XML schema support updated.
  - [x] Option (default off): Machine-wide MSBuild integration. Not recommended any more, use the NuGet (per-project) distribution instead.
  - [x] Removed option: Visual Studio 2015 integration. Use the VSIX package directly instead.
- [x] Fixed: 
  - [x] Types in enum and delegate declarations were not analyzed.
  - [x] Constructed generic, array and pointer types are now analyzed recursively.
  - [x] Source and metadata file load errors are now handled gracefully.
- [x] Changed: Roslyn version updated to 1.3.2.
- [x] Changed: Removed NRefactory as a parser choice.

### Upgrading

* To upgrade please uninstall the previous version first. 
  * Your existing config.nsdepcop files will be preserved.
* Then install the new version. 
  * Recommended: change to per-project MSBuild integration with NuGet. See [README.md](README.md) for details.
  * Please note that if you don't uninstall the previous version then its machine-wide MSBuild integration will override the per-project integration provided by the NuGet package and the old analyzer version will run at build time.

## v1.5
(06/2016)

- [x] Supports Visual Studio 2015 only.
- [x] New: Use the VisibleMembers element to fine-tune the allowed dependencies at the type level.
- [x] New: Added config.nsdepcop XML schema support to Visual Studio so it can validate config syntax and provide IntelliSense.
- [x] Fixed: adding an nsdepcop.config file to a project now works for all C# projects (incl. portable lib).
- [x] Changed: Roslyn version updated to 1.2.2.

## v1.4
(08/2015)

- [x] Supports Visual Studio 2015 only.
- [x] New: Info messages' level can be configured to suppress/enable them in the MSBuild output.
- [x] New: ChildCanDependOnParentImplicitly config attribute.
- [x] Fixed:
  - [x] MSBuild return code was success even if an error was detected.
  - [x] Extension method declaring type was not checked.
- [x] Changed: Roslyn version updated to 1.0.
- [x] Icons (tadaaa! :)

## v1.3
(01/2015)

- [x] Supports Visual Studio 2013 only.
- [x] New: Disallowed rules.

## v1.2
(08/2014)

- [x] Supports Visual Studio 2013 only.
- [x] Requires the Roslyn End User Preview of April 2014.

## v1.1
(07/2013)

- [x] Supports Visual Studio 2012 only.
- [x] Requires Roslyn September 2012 CTP. 
- [x] New: Added NRefactory as the default parser for the MSBuild task.

## v1.0
(03/2013)

- [x] Supports Visual Studio 2012 only.
- [x] Requires Roslyn September 2012 CTP. 