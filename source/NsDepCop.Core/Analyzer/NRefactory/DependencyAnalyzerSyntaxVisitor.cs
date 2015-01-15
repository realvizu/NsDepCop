using Codartis.NsDepCop.Core.Common;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.Semantics;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Codartis.NsDepCop.Core.Analyzer.NRefactory
{
    /// <summary>
    /// Traverses a syntax tree and collects dependency violations.
    /// </summary>
    internal class DependencyAnalyzerSyntaxVisitor : DepthFirstAstVisitor
    {
        /// <summary>
        /// Represents the project that the analysis is working on.
        /// </summary>
        private readonly ICompilation _compilation;

        /// <summary>
        /// The syntax tree that this visitor operates on.
        /// </summary>
        private readonly SyntaxTree _syntaxTree;

        /// <summary>
        /// The configuration of the tool. Containes the dependency rules.
        /// </summary>
        private readonly NsDepCopConfig _config;

        /// <summary>
        /// The validator that decides whether a dependency is allowed.
        /// </summary>
        private readonly DependencyValidator _dependencyValidator;

        /// <summary>
        /// The collection of dependency violations that the syntax visitor found.
        /// </summary>
        public List<DependencyViolation> DependencyViolations { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="compilation">The representation of the current project.</param>
        /// <param name="syntaxTree">The syntax tree that this visitor operates on.</param>
        /// <param name="config">The configuration of the tool.</param>
        /// <param name="dependencyValidator">The validator that decides whether a dependency is allowed.</param>
        public DependencyAnalyzerSyntaxVisitor(ICompilation compilation, SyntaxTree syntaxTree, NsDepCopConfig config, DependencyValidator dependencyValidator)
        {
            if (compilation == null)
                throw new ArgumentNullException("compilation");

            if (syntaxTree == null)
                throw new ArgumentNullException("syntaxTree");

            if (config == null)
                throw new ArgumentNullException("config");

            if (dependencyValidator == null)
                throw new ArgumentNullException("dependencyValidator");

            _compilation = compilation;
            _syntaxTree = syntaxTree;
            _config = config;
            _dependencyValidator = dependencyValidator;

            DependencyViolations = new List<DependencyViolation>();
        }

        public override void VisitIdentifier(Identifier identifier)
        {
            var dependencyViolation = AnalyzeSyntaxNode(identifier);
            if (dependencyViolation != null && DependencyViolations.Count < _config.MaxIssueCount)
                DependencyViolations.Add(dependencyViolation);
        }

        /// <summary>
        /// Performs namespace dependency analysis for a syntax tree node.
        /// </summary>
        /// <param name="node">A syntax tree node.</param>
        /// <returns>A DependencyViolation if an issue was found. Null if no problem.</returns>
        private DependencyViolation AnalyzeSyntaxNode(AstNode node)
        {
            var resolver = new CSharpAstResolver(_compilation, _syntaxTree);

            // Determine the type of the symbol represented by the current syntax node.
            var referencedType = DetermineReferencedType(node, resolver);
            if (referencedType == null || referencedType.Namespace == null)
                return null;

            // Determine the type that contains the current syntax node.
            var enclosingType = DetermineEnclosingType(node, resolver);
            if (enclosingType == null || enclosingType.Namespace == null)
                return null;

            // Get containing namespace for the declaring and the referenced type, in string format.
            var from = enclosingType.Namespace;
            var to = referencedType.Namespace;

            // No rule needed to access the same namespace.
            if (from == to)
                return null;

            // Check the rules whether this dependency is allowed.
            if (_dependencyValidator.IsAllowedDependency(from, to))
                return null;

            // Create a result item for a dependency violation.
            return CreateDependencyViolation(node, new Dependency(from, to), enclosingType, referencedType, _syntaxTree.FileName);
        }

        /// <summary>
        /// Determines the type referred to by the symbol that is represented by the given syntax node. 
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="resolver">A resolver object.</param>
        /// <returns>The type referenced by the given given syntax node.</returns>
        private static IType DetermineReferencedType(AstNode node, CSharpAstResolver resolver)
        {
            // In NRefactory identifier nodes don't have type. So we have to check the identifier's parent.
            var parentNode = node.Parent;

            // These types indicate that the identifier is a declaration, not a usage, so nothing to check here. 
            if (parentNode is EntityDeclaration ||
                parentNode is NamespaceDeclaration ||
                parentNode is UsingDeclaration ||
                parentNode is UsingAliasDeclaration ||
                parentNode is VariableInitializer)
                return null;

            // Special case: if the identifier is in a member access which is the target of an invocation
            // then we have to check the type of the invocation, not the member access.
            if (parentNode is MemberReferenceExpression
                && parentNode.Role == Roles.TargetExpression
                && parentNode.Parent is InvocationExpression)
                parentNode = parentNode.Parent;

            return DetermineTypeOfAstNode(parentNode, resolver);
        }

        /// <summary>
        /// Determines the type of the type declaration that contains the given syntax node.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        /// <param name="resolver">A resolver object.</param>
        /// <returns>The type that contains the given syntax node. Null if not found.</returns>
        private static IType DetermineEnclosingType(AstNode node, CSharpAstResolver resolver)
        {
            // Find the type declaration that contains the current syntax node.
            var typeDeclarationSyntaxNode = node.Ancestors.FirstOrDefault(i => i is TypeDeclaration);
            return typeDeclarationSyntaxNode == null 
                ? null 
                : DetermineTypeOfAstNode(typeDeclarationSyntaxNode, resolver);
        }

        /// <summary>
        /// Resolves the type of the symbol represented by a given syntax tree node.
        /// </summary>
        /// <param name="node">A syntax tree node.</param>
        /// <param name="resolver">A resolver object.</param>
        /// <returns>The type of the symbol represented by a given syntax tree node. Null if failed.</returns>
        private static IType DetermineTypeOfAstNode(AstNode node, CSharpAstResolver resolver)
        {
            var resolveResult = resolver.Resolve(node);

            if (resolveResult != null
                && !resolveResult.IsError
                && TypeWasResolved(resolveResult)
                && IsUserDefinedType(resolveResult.Type))
                return resolveResult.Type;

            return null;
        }

        /// <summary>
        /// Returns a value indicating whether a type was resolved by the given ResolveResult.
        /// </summary>
        /// <param name="resolveResult">A ResolveResult.</param>
        /// <returns>True if a type was resolved by the given ResolveResult.</returns>
        private static bool TypeWasResolved(ResolveResult resolveResult)
        {
            return TypeCanBeResolved(resolveResult)
                && resolveResult.Type != null;
        }

        /// <summary>
        /// Returns a value indicating whether a type can be resolved from the given ResolveResult.
        /// </summary>
        /// <param name="resolveResult">A ResolveResult.</param>
        /// <returns>True if a type can be resolved from the given ResolveResult</returns>
        private static bool TypeCanBeResolved(ResolveResult resolveResult)
        {
            return resolveResult is TypeResolveResult
                || resolveResult is MemberResolveResult;
        }

        /// <summary>
        /// Returns a value indicating whether the given NRefactory type is user defined.
        /// </summary>
        /// <param name="type">An NRefactory type.</param>
        /// <returns>True if the given NRefactory type is user defined.</returns>
        private static bool IsUserDefinedType(IType type)
        {
            return type.Kind == TypeKind.Class
                || type.Kind == TypeKind.Interface
                || type.Kind == TypeKind.Struct
                || type.Kind == TypeKind.Enum
                || type.Kind == TypeKind.Delegate;
        }

        /// <summary>
        /// Creates a dependency violations object.
        /// </summary>
        /// <param name="syntaxNode">The syntax node where the violation was detected.</param>
        /// <param name="illegalDependency">The illegal namespace dependency.</param>
        /// <param name="referencingType">The referencing type.</param>
        /// <param name="referencedType">The referenced type.</param>
        /// <param name="filename">The full path of the source file.</param>
        /// <returns></returns>
        private static DependencyViolation CreateDependencyViolation(
            AstNode syntaxNode, Dependency illegalDependency, IType referencingType, IType referencedType, string filename)
        {
            var sourceSegment = new SourceSegment
            (
                syntaxNode.StartLocation.Line,
                syntaxNode.StartLocation.Column,
                syntaxNode.EndLocation.Line,
                syntaxNode.EndLocation.Column,
                syntaxNode.GetText(),
                filename
            );

            return new DependencyViolation(illegalDependency, referencingType.FullName, referencedType.FullName, sourceSegment);
        }
    }
}
