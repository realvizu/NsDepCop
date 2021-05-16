using System;
using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codartis.NsDepCop.ParserAdapter
{
    /// <summary>
    /// Implements a syntax visitor that traverses the syntax tree and finds all type dependencies. 
    /// </summary>
    /// <remarks>
    /// WARNING: when adding a new SyntaxNode type to the visitor,
    /// also add it to NsDepCopDiagnosticAnalyzerBase.GetSyntaxKindsToRegister
    /// </remarks>
    public class TypeDependencyEnumeratorSyntaxVisitor : CSharpSyntaxVisitor
    {
        /// <summary>
        /// The semantic model of the current document.
        /// </summary>
        private readonly SemanticModel _semanticModel;

        /// <summary>
        /// Implements the analysis logic for a single syntax node.
        /// </summary>
        private readonly ISyntaxNodeAnalyzer _syntaxNodeAnalyzer;

        /// <summary>
        /// The collection of type dependencies that the syntax visitor found.
        /// </summary>
        private readonly List<TypeDependency> _typeDependencies;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="semanticModel">The semantic model for the document.</param>
        /// <param name="syntaxNodeAnalyzer">Syntax node analyzer logic.</param>
        public TypeDependencyEnumeratorSyntaxVisitor(SemanticModel semanticModel, ISyntaxNodeAnalyzer syntaxNodeAnalyzer)
        {
            _semanticModel = semanticModel ?? throw new ArgumentNullException(nameof(semanticModel));
            _syntaxNodeAnalyzer = syntaxNodeAnalyzer ?? throw new ArgumentNullException(nameof(syntaxNodeAnalyzer));

            _typeDependencies = new List<TypeDependency>();
        }

        public IEnumerable<TypeDependency> TypeDependencies => _typeDependencies;

        /// <summary>
        /// Visits all child nodes of a given node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        public override void DefaultVisit(SyntaxNode node)
        {
            foreach (var childNode in node.ChildNodes())
                Visit(childNode);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            AnalyzeNode(node);
            // No need to call DefaultVisit, because it cannot have such child nodes that need to be checked.
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            AnalyzeNode(node);
            DefaultVisit(node);
        }

        /// <summary>
        /// Collects type dependencies for a given syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        protected void AnalyzeNode(SyntaxNode node)
        {
            var typeDependencies = _syntaxNodeAnalyzer.GetTypeDependencies(node, _semanticModel);
            _typeDependencies.AddRange(typeDependencies);
        }
    }
}
