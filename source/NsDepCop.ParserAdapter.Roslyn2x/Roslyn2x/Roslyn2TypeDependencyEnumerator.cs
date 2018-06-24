using Codartis.NsDepCop.Core.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Codartis.NsDepCop.ParserAdapter.Roslyn2x
{
    /// <summary>
    /// Enumerates type dependencies in source code using Roslyn 2.x
    /// </summary>
    public class Roslyn2TypeDependencyEnumerator : RoslynTypeDependencyEnumeratorBase
    {
        public Roslyn2TypeDependencyEnumerator(MessageHandler traceMessageHandler)
            : base(new Roslyn2SyntaxNodeAnalyzer(), traceMessageHandler)
        {
        }

        protected override CSharpParseOptions ParseOptions => new CSharpParseOptions(LanguageVersion.Latest);

        protected override TypeDependencyEnumeratorSyntaxVisitor CreateSyntaxVisitor(SemanticModel semanticModel, ISyntaxNodeAnalyzer syntaxNodeAnalyzer)
            => new Roslyn2TypeDependencyEnumeratorSyntaxVisitor(semanticModel, syntaxNodeAnalyzer);
    }
}