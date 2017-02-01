using System;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// Analyzes dependencies based on a config.
    /// </summary>
    public interface IDependencyAnalyzer : IConfigProvider, IDependencyAnalyzerLogic, IDisposable
    {
    }
}