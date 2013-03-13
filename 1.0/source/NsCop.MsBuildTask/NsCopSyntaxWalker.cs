using Codartis.NsCop.Core;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;

namespace Codartis.NsCop.MsBuildTask
{
    /// <summary>
    /// Implements a syntax walker that traverses the syntax tree 
    /// and invokes the analysis logic for every eligible node.
    /// </summary>
    public class NsCopSyntaxWalker : SyntaxWalker
    {
        /// <summary>
        /// The semantic model of the current document.
        /// </summary>
        private ISemanticModel _semanticModel;

        /// <summary>
        /// The configuration of the tool. Containes the dependency rules.
        /// </summary>
        private NsCopConfig _config;

        /// <summary>
        /// The collection of dependency violations that the syntax walker found.
        /// </summary>
        public List<DependencyViolation> DependencyViolations { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="semanticModel">The semantic model for the document.</param>
        /// <param name="config">The configuration of the tool.</param>
        public NsCopSyntaxWalker(ISemanticModel semanticModel, NsCopConfig config)
        {
            if (semanticModel == null)
                throw new ArgumentNullException("semanticModel");

            if (config == null)
                throw new ArgumentNullException("config");

            _semanticModel = semanticModel;
            _config = config;

            DependencyViolations = new List<DependencyViolation>();
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            CheckNode(node);
            base.VisitIdentifierName(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            CheckNode(node);
            base.VisitInvocationExpression(node);
        }

        public override void VisitElementAccessExpression(ElementAccessExpressionSyntax node)
        {
            CheckNode(node);
            base.VisitElementAccessExpression(node);
        }

        public override void VisitQueryExpression(QueryExpressionSyntax node)
        {
            CheckNode(node);
            base.VisitQueryExpression(node);
        }

        public override void VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
        {
            CheckNode(node);
            base.VisitAliasQualifiedName(node);
        }

        // The following node types should also be analysed in theory
        // but in practice they don't give much useful information 
        // because they all reside in the System namespace 
        // which is usually allowed to be accessed anyway.
        // They just slow down the analysis, so we ignore them for now.
        // - PredefinedTypeSyntax
        // - LiteralExpressionSyntax
        // - VisitNullableType

        /// <summary>
        /// Performs the analysis on the given node and creates a dependency violation object if needed.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        private void CheckNode(SyntaxNode node)
        {
            var dependencyViolation = DependencyAnalyzer.ProcessSyntaxNode(node, _semanticModel, _config);
            if (dependencyViolation != null)
                DependencyViolations.Add(dependencyViolation);
        }
    }
}
