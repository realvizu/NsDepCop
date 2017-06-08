using System;
using System.Collections.Concurrent;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Finds the C# project file (csproj) that a source file belongs to and caches the results.
    /// </summary>
    public class CachingCsprojResolver : ICsprojResolver
    {
        /// <summary>
        /// The resolver that this class delegates to.
        /// </summary>
        private readonly ICsprojResolver _csprojResolver;

        /// <summary>
        /// Caches which source file path resolves to which csproj file path.
        /// </summary>
        private readonly ConcurrentDictionary<string, string> _csprojResolverCache;

        public CachingCsprojResolver(ICsprojResolver csprojResolver)
        {
            if (csprojResolver == null)
                throw new ArgumentNullException(nameof(csprojResolver));

            _csprojResolver = csprojResolver;
            _csprojResolverCache = new ConcurrentDictionary<string, string>();
        }

        public string GetCsprojFile(string sourceFilePath, string assemblyName)
        {
            return _csprojResolverCache.GetOrAdd(sourceFilePath, i => _csprojResolver.GetCsprojFile(i, assemblyName));
        }
    }
}
