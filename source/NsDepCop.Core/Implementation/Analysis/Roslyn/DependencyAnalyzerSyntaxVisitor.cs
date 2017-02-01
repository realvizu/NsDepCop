using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.Roslyn
{
    /// <summary>
    /// Implements a syntax visitor that traverses the syntax tree
    /// and invokes the dependency analysis logic for every eligible node. 
    /// </summary>
    internal class DependencyAnalyzerSyntaxVisitor : CSharpSyntaxVisitor<List<DependencyViolation>>
    {
        /// <summary>
        /// The semantic model of the current document.
        /// </summary>
        private readonly SemanticModel _semanticModel;

        /// <summary>
        /// The validator that decides whether a dependency is allowed.
        /// </summary>
        private readonly ITypeDependencyValidator _typeDependencyValidator;

        /// <summary>
        /// The maximum number of issues to report before stopping analysis.
        /// </summary>
        private readonly int _maxIssueCount;

        /// <summary>
        /// The collection of dependency violations that the syntax visitor found.
        /// </summary>
        private readonly List<DependencyViolation> _dependencyViolations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="semanticModel">The semantic model for the document.</param>
        /// <param name="typeDependencyValidator">The validator that decides whether a dependency is allowed.</param>
        /// <param name="maxIssueCount">The maximum number of issues to report before stopping analysis.</param>
        public DependencyAnalyzerSyntaxVisitor(SemanticModel semanticModel, ITypeDependencyValidator typeDependencyValidator, int maxIssueCount)
        {
            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            if (typeDependencyValidator == null)
                throw new ArgumentNullException(nameof(typeDependencyValidator));

            _semanticModel = semanticModel;
            _typeDependencyValidator = typeDependencyValidator;
            _maxIssueCount = maxIssueCount;

            _dependencyViolations = new List<DependencyViolation>();
        }

        /// <summary>
        /// Visits all child nodes of a given node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override List<DependencyViolation> DefaultVisit(SyntaxNode node)
        {
            foreach (var childNode in node.ChildNodes())
            {
                if (_dependencyViolations.Count < _maxIssueCount)
                    Visit(childNode);
            }

            return _dependencyViolations;
        }

        public override List<DependencyViolation> VisitIdentifierName(IdentifierNameSyntax node)
        {
            return AnalyzeNode(node);
            // No need to call DefaultVisit, because it cannot have such child nodes that need to be checked.
        }

        public override List<DependencyViolation> VisitGenericName(GenericNameSyntax node)
        {
            AnalyzeNode(node);
            return DefaultVisit(node);
        }

        /// <summary>
        /// Performs the analysis on the given node and creates a dependency violation object if needed.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <returns>A list of dependency violations. Can be empty.</returns>
        private List<DependencyViolation> AnalyzeNode(SyntaxNode node)
        {
            var newDependencyViolations = SyntaxNodeAnalyzer.Analyze(node, _semanticModel, _typeDependencyValidator).ToList();
            if (!newDependencyViolations.Any() || _dependencyViolations.Count >= _maxIssueCount)
                return _dependencyViolations;

            var maxElementsToAdd = Math.Min(_maxIssueCount - _dependencyViolations.Count, newDependencyViolations.Count);
            _dependencyViolations.AddRange(newDependencyViolations.Take(maxElementsToAdd));
            return _dependencyViolations;
        }
    }
}
