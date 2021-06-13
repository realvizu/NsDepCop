# NsDepCop Troubleshooting

* [Exception: Unable to communicate with NsDepCop service](#item5)
* [NsDepCop NuGet package is not adding config.nsdepcop file to the project](#item4)
* [Anonymous types raise false alarms](#item3)

<a name="item5"></a>
## Exception: Unable to communicate with NsDepCop service
> Applies only to versions before v2.0.

This problem is either caused by a bug or by the analyzer client not waiting enough time for the analyzer server started in a separate process to spin up.
To fix it:
* Update to v1.10.1 or later. 
* Try to set longer and/or more wait intervals in your config.nsdepcop file(s) by adding the AnalyzerServiceCallRetryTimeSpans attribute to the root element and fiddling with its value. The value should be a comma separated list of wait times between retries (in milliseconds).
  * E.g. this config waits 100ms, then 1sec, then 10sec:

```xml
<NsDepCopConfig AnalyzerServiceCallRetryTimeSpans="100,1000,10000">
```

<a name="item4"></a>
## NsDepCop NuGet package is not adding config.nsdepcop file to the project
> Applies only to versions before v2.0.

If the project uses the **PackageReference** package manager format then content files are not added to the project. 
Workaround:
* **Add** a file called **config.nsdepcop** and fill it in using the examples in [Help](Help.md).
* Install the NsDepCop Visual Studio Extension [![Visual Studio extension](https://img.shields.io/badge/Visual%20Studio%20Marketplace-NsDepCop%20VS2017-green.svg)](https://marketplace.visualstudio.com/items?itemName=FerencVizkeleti.NsDepCopVS2017-CodedependencycheckerforC) and then:
  * Right-click on project >> Add >> New Item... >> NsDepCop Config File

<a name="item3"></a>
## Anonymous types raise false alarms

> Applies only to versions before v1.6. 

For anonymous types the compiler generates a class with no namespace so they will belong to the 'global namespace'. If your NsDepCop config does not allow referencing the global namespace (denoted with a single dot) then it will raise an alarm that you may consider a false positive.

To avoid alarms caused by anonymous types you have to add a rule that you allow referencing the global namespace:
```xml
<Allowed From="YourNamespace" To="." />
```

Or to be more lax:
```xml
<Allowed From="*" To="." />
```
