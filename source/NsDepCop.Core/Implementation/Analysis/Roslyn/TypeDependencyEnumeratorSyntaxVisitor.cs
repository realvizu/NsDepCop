using System;
using System.Collections.Generic;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn
{
    /// <summary>
    /// Implements a syntax visitor that traverses the syntax tree and finds all type dependencies. 
    /// </summary>
    internal class TypeDependencyEnumeratorSyntaxVisitor : CSharpSyntaxVisitor
    {
        /// <summary>
        /// The semantic model of the current document.
        /// </summary>
        private readonly SemanticModel _semanticModel;

        /// <summary>
        /// The collection of type dependencies that the syntax visitor found.
        /// </summary>
        private readonly List<TypeDependency> _typeDependencies;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="semanticModel">The semantic model for the document.</param>
        public TypeDependencyEnumeratorSyntaxVisitor(SemanticModel semanticModel)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            _semanticModel = semanticModel;

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
        private void AnalyzeNode(SyntaxNode node)
        {
            _typeDependencies.AddRange(SyntaxNodeTypeDependencyEnumerator.GetTypeDependencies(node, _semanticModel));
        }
    }
}
