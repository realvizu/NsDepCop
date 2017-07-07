using System;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Retrieves refreshable dependency analyzers and caches them for a certain time span.
    /// </summary>
    public class CachingAnalyzerProvider : TimeBasedCacheBase<string, IRefreshableDependencyAnalyzer>, IAnalyzerProvider
    {
        private readonly IAnalyzerProvider _analyzerProvider;

        public CachingAnalyzerProvider(IAnalyzerProvider analyzerProvider, IDateTimeProvider dateTimeProvider, TimeSpan cacheTimeSpan)
            : base(dateTimeProvider, cacheTimeSpan)
        {
            _analyzerProvider = analyzerProvider ?? throw new ArgumentNullException(nameof(analyzerProvider));
        }

        public void Dispose() => _analyzerProvider?.Dispose();

        public IRefreshableDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath) => GetOrAdd(csprojFilePath);

        protected override IRefreshableDependencyAnalyzer RetrieveItemToCache(string key) => _analyzerProvider.GetDependencyAnalyzer(key);
    }
}
