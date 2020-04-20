using System.Collections.Generic;

namespace Codartis.NsDepCop.ConsoleHost
{
    public struct CompilationInfo
    {
        public IEnumerable<string> SourceFilePaths {get; }
        public IEnumerable<string> ReferencedAssemblyPaths { get; }

        public CompilationInfo(IEnumerable<string> sourceFilePaths, IEnumerable<string> referencedAssemblyPaths)
        {
            SourceFilePaths = sourceFilePaths;
            ReferencedAssemblyPaths = referencedAssemblyPaths;
        }
    }
}
