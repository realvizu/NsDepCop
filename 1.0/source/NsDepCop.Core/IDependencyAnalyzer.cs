using System.Collections.Generic;

namespace Codartis.NsDepCop.Core
{
    /// <summary>
    /// Defines the responsibilities of a type that analyses code and finds dependency violations.
    /// </summary>
    public interface IDependencyAnalyzer
    {
        /// <summary>
        /// Gets the name of the parser.
        /// </summary>
        string ParserName { get; }

        /// <summary>
        /// Analyses a project (source files and referenced assemblies) and returns the found dependency violations.
        /// </summary>
        /// <param name="baseDirectory">The full path of the base directory of the project.</param>
        /// <param name="sourceFilePaths">A collection of the full path of source files.</param>
        /// <param name="referencedAssemblyPaths">A collection of the full path of referenced assemblies.</param>
        /// <returns>A collection of dependency violations. Empty collection if none found.</returns>
        IEnumerable<DependencyViolation> AnalyzeProject(
            string baseDirectory, IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths);
    }
}
