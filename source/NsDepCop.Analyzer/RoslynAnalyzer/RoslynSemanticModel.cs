using Codartis.NsDepCop.Interface.Analysis;
using Codartis.NsDepCop.Util;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.RoslynAnalyzer
{
    /// <summary>
    /// Wraps a Roslyn semantic model object.
    /// </summary>
    public class RoslynSemanticModel : ObjectWrapper<SemanticModel>, ISemanticModel
    {
        public RoslynSemanticModel(SemanticModel value) 
            : base(value)
        {
        }
    }
}
