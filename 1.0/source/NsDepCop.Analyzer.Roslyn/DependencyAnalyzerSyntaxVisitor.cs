using Codartis.NsDepCop.Core;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;

namespace Codartis.NsDepCop.Analyzer.Roslyn
{
    /// <summary>
    /// Implements a syntax visitor that traverses the syntax tree 
    /// and invokes the dependency analysis logic for every eligible node.
    /// </summary>
    internal class DependencyAnalyzerSyntaxVisitor : SyntaxVisitor<List<DependencyViolation>>
    {
        /// <summary>
        /// The semantic model of the current document.
        /// </summary>
        private ISemanticModel _semanticModel;

        /// <summary>
        /// The configuration of the tool. Containes the dependency rules.
        /// </summary>
        private NsDepCopConfig _config;

        /// <summary>
        /// The collection of dependency violations that the syntax visitor found.
        /// </summary>
        private List<DependencyViolation> _dependencyViolations;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="semanticModel">The semantic model for the document.</param>
        /// <param name="config">The configuration of the tool.</param>
        public DependencyAnalyzerSyntaxVisitor(ISemanticModel semanticModel, NsDepCopConfig config)
        {
            if (semanticModel == null)
                throw new ArgumentNullException("semanticModel");

            if (config == null)
                throw new ArgumentNullException("config");

            _semanticModel = semanticModel;
            _config = config;
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
            return CheckNode(node);
            // No need to call DefaultVisit, because it cannot have such child nodes that need to be checked.
        }

        public override List<DependencyViolation> VisitQualifiedName(QualifiedNameSyntax node)
        {
            return CheckNode(node);
            // No need to call DefaultVisit, because it cannot have such child nodes that need to be checked.
        }

        public override List<DependencyViolation> VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
        {
            return CheckNode(node);
            // No need to call DefaultVisit, because it cannot have such child nodes that need to be checked.
        }

        public override List<DependencyViolation> VisitGenericName(GenericNameSyntax node)
        {
            CheckNode(node);
            return DefaultVisit(node);
        }

        public override List<DependencyViolation> VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            CheckNode(node);
            return DefaultVisit(node);
        }

        // No need to check the following node types.
        //   AnonymousMethodExpressionSyntax: its block's return type is checked anyway.
        //   ArrayTypeSyntax: its element type is checked.
        //   BaseExpressionSyntax: the inherited types are checked at the inheritor's declaration.
        //   MemberAccessExpressionSyntax: it contains the member name that is checked anyway.
        //   NullableTypeSyntax: its underlying type's name is checked.

        // The following node types rarely yield useful result in real life scenarios.
        // On the other hand it greatly speeds up the process to omit these, so these are not analyzed.
        //   LiteralExpressionSyntax: Finds dependency only on System namespace.
        //   PredefinedTypeSyntax: Finds dependency only on System namespace.
        //   AnonymousObjectCreationExpressionSyntax: Finds dependency only on the global namespace.
        //   ConditionalExpressionSyntax: Finds the same type as its children.
        //   QueryExpressionSyntax: Finds the same type as its children or dependency on System.Linq (eg. IGrouping).
        //   BinaryExpressionSyntax: Finds the same type as its children except for some rare operator overloading.
        //   PostfixUnaryExpressionSyntax: Finds the same type as its child except for some rare operator overloading.
        //   PrefixUnaryExpressionSyntax: Finds the same type as its child except for some rare operator overloading.
        //   ElementAccessExpressionSyntax: Finds the same type as its child except for some rare indexer overloading.

        /// <summary>
        /// Performs the analysis on the given node and creates a dependency violation object if needed.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        private List<DependencyViolation> CheckNode(SyntaxNode node)
        {
            var dependencyViolation = SyntaxNodeAnalyzer.ProcessSyntaxNode(node, _semanticModel, _config);
            if (dependencyViolation != null && _dependencyViolations.Count < _config.MaxIssueCount)
                _dependencyViolations.Add(dependencyViolation);

            return _dependencyViolations;
        }
    }
}
