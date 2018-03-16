using System;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.ParserAdapter.Roslyn2x;

namespace Codartis.NsDepCop.ServiceHost
{
    /// <summary>
    /// Implements dependency analyzer service as a remoting server.
    /// </summary>
    public class RemoteDependencyAnalyzerServer : MarshalByRefObject, IRemoteDependencyAnalyzer
    {
        private readonly AnalyzeProjectResultBuilder _resultBuilder;
        private readonly IDependencyAnalyzerFactory _dependencyAnalyzerFactory;

        public RemoteDependencyAnalyzerServer()
        {
            _resultBuilder = new AnalyzeProjectResultBuilder();
            var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(LogTrace);
            _dependencyAnalyzerFactory = new DependencyAnalyzerFactory(typeDependencyEnumerator, LogTrace);
        }

        public AnalyzerMessageBase[] AnalyzeProject(IAnalyzerConfig config, string[] sourcePaths, string[] referencedAssemblyPaths)
        {
            var resultBuilder = new AnalyzeProjectResultBuilder();

            var dependencyAnalyzer = _dependencyAnalyzerFactory.Create(config);
            var illegalDependencies = dependencyAnalyzer.AnalyzeProject(sourcePaths, referencedAssemblyPaths);

            foreach (var illegalDependency in illegalDependencies)
                resultBuilder.AddIllegalDependency(illegalDependency);

            return resultBuilder.ToArray();
        }

        private void LogTrace(string i) => _resultBuilder.AddTrace(i);
    }
}
