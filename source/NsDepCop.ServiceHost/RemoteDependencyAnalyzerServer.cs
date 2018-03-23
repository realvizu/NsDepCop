using System;
using Codartis.NsDepCop.Core.Factory;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.ParserAdapter.Roslyn2x;

namespace Codartis.NsDepCop.ServiceHost
{
    /// <summary>
    /// Implements dependency analyzer service as a remoting server. Stateless.
    /// </summary>
    public class RemoteDependencyAnalyzerServer : MarshalByRefObject, IRemoteDependencyAnalyzer
    {
        public AnalyzerMessageBase[] AnalyzeProject(IAnalyzerConfig config, string[] sourcePaths, string[] referencedAssemblyPaths)
        {
            var resultBuilder = new AnalyzeProjectResultBuilder();

            var analyzerFactory = new DependencyAnalyzerFactory(config, resultBuilder.AddTrace);
            var typeDependencyEnumerator = new Roslyn2TypeDependencyEnumerator(resultBuilder.AddTrace);
            var dependencyAnalyzer = analyzerFactory.CreateInProcess(typeDependencyEnumerator);
            var illegalDependencies = dependencyAnalyzer.AnalyzeProject(sourcePaths, referencedAssemblyPaths);

            foreach (var illegalDependency in illegalDependencies)
                resultBuilder.AddIllegalDependency(illegalDependency);

            return resultBuilder.ToArray();
        }
    }
}
