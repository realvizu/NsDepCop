using System;
using Codartis.NsDepCop.Core.Interface.Config;

namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// An analyzer that manages its own config.
    /// </summary>
    public interface IConfiguredAnalyzer : IConfigProvider, IDependencyAnalyzer, IDisposable
    {
    }
}