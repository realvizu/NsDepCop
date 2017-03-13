using System;
using System.Collections.Concurrent;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Finds the C# project file (csproj) that a source file belongs to and caches the results.
    /// </summary>
    internal class CachingCsprojResolver : ICsprojResolver
    {
        /// <summary>
        /// The resolver that this class delegates to.
        /// </summary>
        private readonly CsprojResolver _csprojResolver;

        /// <summary>
        /// Caches which source file path resolves to which csproj file path.
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _csprojResolverCache;

        public CachingCsprojResolver(Action<string> diagnosticMessageHandler = null)
        {
            _csprojResolver = new CsprojResolver(diagnosticMessageHandler);
            _csprojResolverCache = new ConcurrentDictionary<string, string>();
        }

        public string GetCsprojFile(string sourceFilePath, string assemblyName)
        {
            return _csprojResolverCache.GetOrAdd(sourceFilePath, i => _csprojResolver.GetCsprojFile(i, assemblyName));
        }
    }
}
