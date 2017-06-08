using Codartis.NsDepCop.Core.Interface.Analysis;
using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.VisualStudioIntegration
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
