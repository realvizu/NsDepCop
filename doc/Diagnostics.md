# NsDepCop Diagnostics Reference

### NSDEPCOP01

Title|Illegal namespace reference.
:-|:-
Severity|Set by CodeIssueKind attribute in the config file. (Default: Warning)
Explanation|The designated symbol in the source code implies a namespace reference which is not allowed by the current namespace dependency configuration. That is, the symbol is used in a type in namespace A and references a type in namespace B but A->B dependency is not allowed.
To Do|Change the namespace dependency configuration in the config.nsdepcop file or change your code/design to avoid this namespace reference.

### NSDEPCOP02

Title|Too many issues, analysis was stopped.
:-|:-
Severity|Warning
Explanation|The analysis stops after reporting a certain number of issues (default: 100) to avoid flooding your build log.
To Do|Correct the reported issues and run the build again. Or set the MaxIssueCount attribute in your config.nsdepcop to a higher number.

### NSDEPCOP03

Title|No config file found, analysis skipped.
:-|:-
Severity|Info
Explanation|If there's no config.nsdepcop file next to the C# project file then NsDepCop does not perform analysis.
To Do|None, this is just an informational message.

### NSDEPCOP04

Title|Analysis is disabled in the nsdepcop config file.
:-|:-
Severity|Info
Explanation|There is a config.nsdepcop file next to the C# project file, but it contains IsEnabled="False".
To Do|None, this is just an informational message.

### NSDEPCOP05

Title|Error loading NsDepCop config.
:-|:-
Severity|Error
Explanation|There was an error while loading the config.nsdepcop file. Some possible reasons: malformed content, file permission or locking problem. The diagnostic contains the actual exception message.
To Do|Make sure that the file can be read by the user running the build or Visual Studio and make sure that its content is correct.
