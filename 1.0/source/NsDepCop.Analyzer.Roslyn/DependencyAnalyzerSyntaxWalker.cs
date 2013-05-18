using Codartis.NsDepCop.Core;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;

namespace Codartis.NsDepCop.Analyzer.Roslyn
{
    /// <summary>
    /// Implements a syntax walker that traverses the syntax tree 
    /// and invokes the dependency analysis logic for every eligible node.
    /// </summary>
    public class DependencyAnalyzerSyntaxWalker : SyntaxWalker
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
        /// The collection of dependency violations that the syntax walker found.
        /// </summary>
        public List<DependencyViolation> DependencyViolations { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="semanticModel">The semantic model for the document.</param>
        /// <param name="config">The configuration of the tool.</param>
        public DependencyAnalyzerSyntaxWalker(ISemanticModel semanticModel, NsDepCopConfig config)
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
            // No need to call base, because it cannot have such child nodes that need to be checked.
        }

        public override void VisitQualifiedName(QualifiedNameSyntax node)
        {
            CheckNode(node);
            // No need to call base, because it cannot have such child nodes that need to be checked.
        }

        public override void VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
        {
            CheckNode(node);
            // No need to call base, because it cannot have such child nodes that need to be checked.
        }

        public override void VisitGenericName(GenericNameSyntax node)
        {
            CheckNode(node);
            if (DependencyViolations.Count < Constants.MAX_ISSUE_REPORTED_PER_FILE)
                base.VisitGenericName(node);
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            CheckNode(node);
            if (DependencyViolations.Count < Constants.MAX_ISSUE_REPORTED_PER_FILE)
                base.VisitInvocationExpression(node);
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
        private void CheckNode(SyntaxNode node)
        {
            if (DependencyViolations.Count >= Constants.MAX_ISSUE_REPORTED_PER_FILE)
                return;

            var dependencyViolation = SyntaxNodeAnalyzer.ProcessSyntaxNode(node, _semanticModel, _config);
            if (dependencyViolation != null)
                DependencyViolations.Add(dependencyViolation);
        }
    }
}
