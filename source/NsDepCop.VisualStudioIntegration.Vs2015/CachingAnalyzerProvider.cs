using System;
using Codartis.NsDepCop.Core.Interface.Analysis.Configured;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Retrieves configured dependency analyzers and caches them for a certain time span.
    /// </summary>
    public sealed class CachingAnalyzerProvider : TimeBasedCacheBase<string, IConfiguredDependencyAnalyzer>, IAnalyzerProvider
    {
        private readonly IAnalyzerProvider _analyzerProvider;

        public CachingAnalyzerProvider(IAnalyzerProvider analyzerProvider, IDateTimeProvider dateTimeProvider, TimeSpan cacheTimeSpan)
            : base(dateTimeProvider, cacheTimeSpan)
        {
            _analyzerProvider = analyzerProvider ?? throw new ArgumentNullException(nameof(analyzerProvider));
        }

        public void Dispose() => _analyzerProvider?.Dispose();

        public IConfiguredDependencyAnalyzer GetDependencyAnalyzer(string csprojFilePath) => GetOrAdd(csprojFilePath);

        protected override IConfiguredDependencyAnalyzer RetrieveItemToCache(string key) => _analyzerProvider.GetDependencyAnalyzer(key);
    }
}
