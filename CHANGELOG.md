# NsDepCop Change Log

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