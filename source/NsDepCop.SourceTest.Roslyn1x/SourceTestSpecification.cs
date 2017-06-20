using System.Runtime.CompilerServices;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.ParserAdapter.Roslyn1x;

namespace Codartis.NsDepCop.SourceTest
{
    internal class SourceTestSpecification : SourceTestSpecificationBase
    {
        private SourceTestSpecification(string name, ITypeDependencyEnumerator typeDependencyEnumerator)
            :base(name, typeDependencyEnumerator)
        {
        }

        public static SourceTestSpecification Create([CallerMemberName] string name = null) 
            => new SourceTestSpecification(name, new Roslyn1TypeDependencyEnumerator(DebugMessageHandler));
    }
}
