using System.Runtime.CompilerServices;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.ParserAdapter.Roslyn;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.SourceTest
{
    internal class SourceTestSpecification : SourceTestSpecificationBase
    {
        protected override CSharpParseOptions CSharpParseOptions => new CSharpParseOptions(LanguageVersion.Latest);

        private SourceTestSpecification(string name, ITypeDependencyEnumerator typeDependencyEnumerator)
            : base(name, typeDependencyEnumerator)
        {
        }

        public static SourceTestSpecification Create([CallerMemberName] string name = null)
            => new(name, new TypeDependencyEnumerator(new SyntaxNodeAnalyzer(), DebugMessageHandler));
    }
}