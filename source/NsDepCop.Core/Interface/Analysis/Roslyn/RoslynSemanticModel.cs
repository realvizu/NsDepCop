using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Core.Interface.Analysis.Roslyn
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
