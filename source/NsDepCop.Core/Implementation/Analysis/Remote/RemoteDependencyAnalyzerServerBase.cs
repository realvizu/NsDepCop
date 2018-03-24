using System;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Interface.Analysis.Remote;
using Codartis.NsDepCop.Core.Interface.Config;
using Codartis.NsDepCop.Core.Util;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Remote
{
    /// <summary>
    /// Abstract base class for a dependency analyzer service implemented as a remoting server. Stateless.
    /// </summary>
    /// <remarks>
    /// This class is abstract because the Core package cannot reference a concrete TypeDependencyEnumerator,
    /// but it cannot get it as a ctor parameter either 
    /// because a server activated remoting server can have only parameterless ctor
    /// so it must acquire the TypeDependencyEnumerator using a template method.
    /// </remarks>
    public abstract class RemoteDependencyAnalyzerServerBase : MarshalByRefObject, IRemoteDependencyAnalyzer
    {
        public AnalyzerMessageBase[] AnalyzeProject(IAnalyzerConfig config, string[] sourcePaths, string[] referencedAssemblyPaths)
        {
            var resultBuilder = new AnalyzeProjectResultBuilder();

            var typeDependencyEnumerator = GetTypeDependencyEnumerator(resultBuilder.AddTrace);
            var dependencyAnalyzer = new DependencyAnalyzer(config, typeDependencyEnumerator, resultBuilder.AddTrace);
            var illegalDependencies = dependencyAnalyzer.AnalyzeProject(sourcePaths, referencedAssemblyPaths);

            foreach (var illegalDependency in illegalDependencies)
                resultBuilder.AddIllegalDependency(illegalDependency);

            return resultBuilder.ToArray();
        }

        protected abstract ITypeDependencyEnumerator GetTypeDependencyEnumerator(MessageHandler traceMessageHandler);
    }
}
