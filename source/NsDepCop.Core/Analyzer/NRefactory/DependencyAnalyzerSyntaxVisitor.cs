using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;

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
        /// The resolver that returns semantic info for syntax tree nodes.
        /// </summary>
        private readonly CSharpAstResolver _resolver;

        /// <summary>
        /// The configuration of the tool. Containes the dependency rules.
        /// </summary>
        private readonly NsDepCopConfig _config;

        /// <summary>
        /// The validator that decides whether a dependency is allowed.
        /// </summary>
        private readonly TypeDependencyValidator _typeDependencyValidator;

        /// <summary>
        /// The collection of dependency violations that the syntax visitor found.
        /// </summary>
        public List<DependencyViolation> DependencyViolations { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="compilation">The representation of the current project.</param>
        /// <param name="syntaxTree">The syntax tree that this visitor operates on.</param>
        /// <param name="config">The configuration of the tool.</param>
        /// <param name="typeDependencyValidator">The validator that decides whether a dependency is allowed.</param>
        public DependencyAnalyzerSyntaxVisitor(ICompilation compilation, SyntaxTree syntaxTree, NsDepCopConfig config, TypeDependencyValidator typeDependencyValidator)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            if (syntaxTree == null)
                throw new ArgumentNullException(nameof(syntaxTree));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (typeDependencyValidator == null)
                throw new ArgumentNullException(nameof(typeDependencyValidator));

            _compilation = compilation;
            _syntaxTree = syntaxTree;
            _config = config;
            _typeDependencyValidator = typeDependencyValidator;

            _resolver = new CSharpAstResolver(_compilation, _syntaxTree);

            DependencyViolations = new List<DependencyViolation>();
        }

        public override void VisitIdentifier(Identifier identifier)
        {
            var newDependencyViolations = AnalyzeSyntaxNode(identifier).ToList();
            if (!newDependencyViolations.Any() || DependencyViolations.Count >= _config.MaxIssueCount)
                return;

            var maxElementsToAdd = Math.Min(_config.MaxIssueCount - DependencyViolations.Count, newDependencyViolations.Count);
            DependencyViolations.AddRange(newDependencyViolations.Take(maxElementsToAdd));
        }

        /// <summary>
        /// Performs namespace dependency analysis for a syntax tree node.
        /// </summary>
        /// <param name="node">A syntax tree node.</param>
        /// <returns>A list of dependency violations. Can be empty.</returns>
        private IEnumerable<DependencyViolation> AnalyzeSyntaxNode(AstNode node)
        {
            // Determine the type that contains the current syntax node.
            var enclosingType = DetermineEnclosingType(node, _resolver);
            if (enclosingType?.Namespace == null)
                yield break;

            // Determine the type referenced by the symbol represented by the current syntax node.
            var referencedType = DetermineReferencedType(node, _resolver);
            var referencedTypeDependencyViolation = ValidateDependency(enclosingType, referencedType, node);
            if (referencedTypeDependencyViolation != null)
                yield return referencedTypeDependencyViolation;

            // If this is an extension method invocation then determine the type declaring the extension method.
            var declaringType = DetermineExtensionMethodDeclaringType(node, _resolver);
            var declaringTypeDependencyViolation = ValidateDependency(enclosingType, declaringType, node);
            if (declaringTypeDependencyViolation != null)
                yield return declaringTypeDependencyViolation;
        }

        /// <summary>
        /// Validates whether a type is allowed to reference another. Returns a DependencyViolation if not allowed.
        /// </summary>
        /// <param name="fromType">The referring type.</param>
        /// <param name="toType">The referenced type.</param>
        /// <param name="node">The syntax node currently analyzed.</param>
        /// <returns>A DependencyViolation if the dependency is not allowed. Null otherwise.</returns>
        private DependencyViolation ValidateDependency(IType fromType, IType toType, AstNode node)
        {
            if (fromType?.Namespace == null || toType?.Namespace == null)
                return null;

            // Get containing namespace for the declaring and the referenced type, in string format.
            var fromNamespace = fromType.Namespace;
            var toNamespace = toType.Namespace;

            // Check the rules whether this dependency is allowed.
            return _typeDependencyValidator.IsAllowedDependency(fromNamespace, fromType.GetMetadataName(), toNamespace, toType.GetMetadataName()) 
                ? null 
                : CreateDependencyViolation(node, new Dependency(fromNamespace, toNamespace), fromType, toType, _syntaxTree.FileName);
        }

        /// <summary>
        /// If the given node is an extension method invocation's identifier then returns the declaring type of the extension method.
        /// </summary>
        /// <param name="node">The currently analyzed AST node.</param>
        /// <param name="resolver">The AST resolver.</param>
        /// <returns>The declaring type of the extension method or null if not applicable.</returns>
        private static IType DetermineExtensionMethodDeclaringType(AstNode node, CSharpAstResolver resolver)
        {
            if (!(node?.Parent?.Parent is InvocationExpression))
                return null;

            var csharpInvocationResolveResult = resolver.Resolve(node.Parent.Parent) as CSharpInvocationResolveResult;
            if (csharpInvocationResolveResult == null  ||
                !csharpInvocationResolveResult.IsExtensionMethodInvocation ||
                csharpInvocationResolveResult.Member == null)
                return null;

            return csharpInvocationResolveResult.Member.DeclaringType;
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

            // For method name nodes their parent resolve result gives the type info we need
            if (resolveResult is MethodGroupResolveResult && node != null)
                resolveResult = resolver.Resolve(node.Parent);

            if (resolveResult != null
                && !resolveResult.IsError
                && IsUserDefinedType(resolveResult.Type))
                return resolveResult.Type;

            return null;
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
                syntaxNode.ToString(),
                filename
            );

            return new DependencyViolation(illegalDependency, referencingType.FullName, referencedType.FullName, sourceSegment);
        }
    }
}
