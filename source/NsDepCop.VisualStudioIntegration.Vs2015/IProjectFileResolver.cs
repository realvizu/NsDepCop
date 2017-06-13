namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Finds project files.
    /// </summary>
    public interface IProjectFileResolver
    {
        /// <summary>
        /// Returns the project files for a given assembly name.
        /// </summary>
        /// <param name="assemblyName">An assembly name.</param>
        /// <returns>The project file with full path the produces the given assembly. Null if not found.</returns>
        string FindByAssemblyName(string assemblyName);
    }
}