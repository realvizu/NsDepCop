namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Finds the C# project file (csproj) that a given source file belongs to.
    /// </summary>
    internal interface ICsprojResolver
    {
        /// <summary>
        /// Returns the path of the csproj that a given source file belongs to.
        /// </summary>
        /// <param name="sourceFilePath">The full path of a source file.</param>
        /// <param name="assemblyName">The name of the assembly that the source file belongs to.</param>
        /// <returns>The path of a csproj file or null if not found.</returns>
        string GetCsprojFile(string sourceFilePath, string assemblyName);
    }
}