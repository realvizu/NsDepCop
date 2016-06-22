using System;
using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Interface
{
    /// <summary>
    /// Defines the responsibilities of an analyzer that finds dependency violations.
    /// </summary>
    public interface IDependencyAnalyzer
    {
        /// <summary>
        /// Gets the name of the parser.
        /// </summary>
        string ParserName { get; }

        /// <summary>
        /// Gets the state of the analyzer.
        /// </summary>
        DependencyAnalyzerState State { get; }

        /// <summary>
        /// Gets the config exception (if any). Returns null if there was no exception.
        /// </summary>
        Exception ConfigException { get; }

        /// <summary>
        /// The severity level of the dependency violation issues.
        /// </summary>
        IssueKind DependencyViolationIssueKind { get; }

        /// <summary>
        /// The number of issues that stops analysis immediately.
        /// </summary>
        int MaxIssueCount { get; }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        IEnumerable<DependencyViolation> AnalyzeProject(
            IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths);
    }
}
