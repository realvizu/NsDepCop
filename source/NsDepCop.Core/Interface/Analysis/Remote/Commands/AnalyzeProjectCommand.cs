namespace Codartis.NsDepCop.Core.Interface.Analysis.Remote.Commands
{
    using System;
    using Codartis.NsDepCop.Core.Interface.Config;

    [Serializable]
    public class AnalyzeProjectCommand : Command<AnalyzeProjectCommand.ParameterType, IRemoteMessage[]>
    {
        [Serializable]
        public class ParameterType
        {
            public IAnalyzerConfig Config { get; set; }
            public string[] SourcePaths { get; set; }
            public string[] ReferencedAssemblyPaths { get; set; }
        }

        public AnalyzeProjectCommand(ParameterType parameters)
            : base("AnalyzeProject", parameters)
        {
        }
    }
}