using System.Threading.Tasks;
using Codartis.NsDepCop.RoslynAnalyzer;
using Codartis.NsDepCop.Test.Verifiers;
using Xunit;

namespace Codartis.NsDepCop.Test.RoslynAnalyzer
{
    public class NsDepCopAnalyzerTests
    {
        [Fact]
        public async Task EmptySource_NoIssues()
        {
            var test = @"";

            await CSharpAnalyzerVerifier<NsDepCopAnalyzer>.VerifyAnalyzerAsync(test);
        }
    }
}
