# NsDepCop Diagnostics Reference

### NSDEPCOP01

Title|Illegal namespace reference.
:-|:-
Default Severity|Warning
Explanation|The designated type cannot reference the other type because their namespaces cannot depend on each other according to the current rules.
To Do|Change the dependency rules in the 'config.nsdepcop' file or change your design to avoid this namespace dependency.

### NSDEPCOP02

Title|Too many dependency issues, analysis was stopped.
:-|:-
Default Severity|Warning
Explanation|The number of dependency issues in this compilation has exceeded the configured maximum value.
To Do|Correct the reported issues and run the build again or set the MaxIssueCount attribute in your 'config.nsdepcop' file to a higher number.

### NSDEPCOP03

Title|No config file found, analysis skipped.
:-|:-
Default Severity|Info
Explanation|This analyzer requires that you add a file called 'config.nsdepcop' to your project with build action 'C# analyzer additional file'.
To Do|None, this is just an informational message.

### NSDEPCOP04

Title|Analysis is disabled in the config file.
:-|:-
Default Severity|Info
Explanation|The IsEnabled attribute was set to false in this project's 'config.nsdepcop' file, so the analyzer skips this project.
To Do|None, this is just an informational message.

### NSDEPCOP05

Title|Error loading config.
:-|:-
Default Severity|Error
Explanation|There was an error while loading the 'config.nsdepcop' file, see the message for details. Some common reasons: malformed content, file permission or file locking problem.
To Do|Make sure that the file can be read by the user running the build or Visual Studio and make sure that its content is correct.

### NSDEPCOP06

Title|Analysis is disabled with environment variable.
:-|:-
Default Severity|Info
Explanation|If the 'DisableNsDepCop' environment variable is set to 'True' or '1' then all analysis is skipped.
To Do|None, this is just an informational message.

### NSDEPCOP07

Title|Illegal assembly reference.
:-|:-
Default Severity|Warning
Explanation|The designated assembly cannot reference the other assembly because their dependency is prohibit according to the current rules.
To Do|Change the dependency rules in the 'config.nsdepcop' file or change your design to avoid this assembly dependency.