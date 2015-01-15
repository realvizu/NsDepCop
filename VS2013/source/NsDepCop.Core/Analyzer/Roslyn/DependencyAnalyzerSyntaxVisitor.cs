﻿using Codartis.NsDepCop.Core.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Analyzer.Roslyn
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
        /// The configuration of the tool.
        /// </summary>
        private readonly NsDepCopConfig _config;

        /// <summary>
        /// The validator that decides whether a dependency is allowed.
        /// </summary>
        private readonly DependencyValidator _dependencyValidator;

        /// <summary>
        /// The collection of dependency violations that the syntax visitor found.
        /// </summary>
        private readonly List<DependencyViolation> _dependencyViolations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="semanticModel">The semantic model for the document.</param>
        /// <param name="config">The configuration of the tool.</param>
        /// <param name="dependencyValidator">The validator that decides whether a dependency is allowed.</param>
        public DependencyAnalyzerSyntaxVisitor(SemanticModel semanticModel, NsDepCopConfig config, DependencyValidator dependencyValidator)
        {
            if (semanticModel == null)
                throw new ArgumentNullException("semanticModel");

            if (config == null)
                throw new ArgumentNullException("config");

            if (dependencyValidator == null)
                throw new ArgumentNullException("dependencyValidator");

            _semanticModel = semanticModel;
            _config = config;
            _dependencyValidator = dependencyValidator;

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
                if (_dependencyViolations.Count < _config.MaxIssueCount)
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
        private List<DependencyViolation> AnalyzeNode(SyntaxNode node)
        {
            var dependencyViolation = SyntaxNodeAnalyzer.Analyze(node, _semanticModel, _dependencyValidator);
            if (dependencyViolation != null && _dependencyViolations.Count < _config.MaxIssueCount)
                _dependencyViolations.Add(dependencyViolation);

            return _dependencyViolations;
        }
    }
}
