using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;

namespace Codartis.NsDepCop.ConsoleHost
{
    internal class NewCsProjParser : ICompilationInfoProvider
    {
        private const string  ResolveReferencesTargetName = "ResolveReferences";

        private readonly string _csProjFilePath;

        public NewCsProjParser(string csProjFilePath)
        {
            _csProjFilePath = csProjFilePath;
        }

        public CompilationInfo GetCompilationInfo()
        {
            var references = GetReferences(_csProjFilePath);

            // TODO: get source files

            return new CompilationInfo(null, references);
        }

        private static IEnumerable<string> GetReferences(string csProjFilePath)
        {
            var projectCollection = new ProjectCollection();
            var buildParameters = new BuildParameters(projectCollection)
            {
                Loggers = new List<Microsoft.Build.Framework.ILogger> {new Logger()}
            };
            var globalProperty = new Dictionary<string, string>
            {
                {"Configuration", "Debug"},
                {"Platform", "AnyCPU"}
            };
            BuildManager.DefaultBuildManager.ResetCaches();
            var buildRequest = new BuildRequestData(csProjFilePath, globalProperty, null, new[] {ResolveReferencesTargetName}, null);
            var buildResult = BuildManager.DefaultBuildManager.Build(buildParameters, buildRequest);

            if (buildResult.OverallResult == BuildResultCode.Failure)
                throw new Exception($"Build result={buildResult.OverallResult}");

            return buildResult.ResultsByTarget[ResolveReferencesTargetName].Items.Select(i => i.ItemSpec).ToList();
        }

        public static bool CanParse(string csProjFilePath)
        {
            var document = XDocument.Load(csProjFilePath);
            if (document.Root == null)
                throw new Exception($"Error loading {csProjFilePath}");

            var sdkAttribute = document.Root.Attribute("Sdk");
            return sdkAttribute != null;
        }
    }
}