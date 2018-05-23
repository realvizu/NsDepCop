using System;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Retrieves configured dependency analyzers and caches them for a certain time span.
    /// </summary>
    public sealed class CachingAnalyzerProvider : TimeBasedCacheBase<string, IDependencyAnalyzer>, IAnalyzerProvider
    {
        private readonly IAnalyzerProvider _analyzerProvider;

        public CachingAnalyzerProvider(IAnalyzerProvider analyzerProvider, IDateTimeProvider dateTimeProvider, TimeSpan cacheTimeSpan)
            : base(dateTimeProvider, cacheTimeSpan)
        {
            _analyzerProvider = analyzerProvider ?? throw new ArgumentNullException(nameof(analyzerProvider));
        }

        public void Dispose() => _analyzerProvider?.Dispose();

        public IDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath) => GetOrAdd(csprojFilePath);

        protected override IDependencyAnalyzer RetrieveItemToCache(string key) => _analyzerProvider.GetDependencyAnalyzer(key);
    }
}
