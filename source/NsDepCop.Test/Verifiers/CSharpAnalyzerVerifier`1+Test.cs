using System.IO;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing.Verifiers;

namespace Codartis.NsDepCop.Test.Verifiers
{
    public static partial class CSharpAnalyzerVerifier<TAnalyzer>
        where TAnalyzer : DiagnosticAnalyzer, new()
    {
        public class Test : CSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
        {
            public Test(string configFilePath)
            {
                SolutionTransforms.Add((solution, projectId) =>
                {
                    var project = solution.GetProject(projectId);
           
                    var compilationOptions = project.CompilationOptions;
                    compilationOptions = compilationOptions.WithSpecificDiagnosticOptions(
                        compilationOptions.SpecificDiagnosticOptions.SetItems(CSharpVerifierHelper.NullableWarnings));

                    var fileName = Path.GetFileName(configFilePath);
                    var text = File.ReadAllText(configFilePath);
                    var textDocument = project.AddAdditionalDocument(fileName, text, filePath: configFilePath);

                    return textDocument.Project.Solution.WithProjectCompilationOptions(projectId, compilationOptions);
                });
            }
        }
    }
}