using System;
using System.Collections.Generic;
using System.Linq;
using Codartis.NsDepCop.Core.Interface.Analysis;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;

namespace Codartis.NsDepCop.Core.Implementation.Analysis.NRefactory
{
    /// <summary>
    /// Traverses a syntax tree and enumerates type dependencies.
    /// </summary>
    internal class TypeDependencyEnumeratorSyntaxVisitor : DepthFirstAstVisitor
    {
        /// <summary>
        /// The syntax tree that this visitor operates on.
        /// </summary>
        private readonly SyntaxTree _syntaxTree;

        /// <summary>
        /// The resolver that returns semantic info for syntax tree nodes.
        /// </summary>
        private readonly CSharpAstResolver _resolver;

        /// <summary>
        /// The collection of type dependencies that the syntax visitor found.
        /// </summary>
        public List<TypeDependency> TypeDependencies { get; }

        /// <summary>
        /// The list of those type kinds that are subject of dependency analysis.
        /// </summary>
        private static readonly List<TypeKind> AnalyzedTypeKinds = new List<TypeKind>
        {
            TypeKind.Class,
            TypeKind.Delegate,
            TypeKind.Enum,
            TypeKind.Interface,
            TypeKind.Struct,
            TypeKind.TypeParameter
        };

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="compilation">The representation of the current project.</param>
        /// <param name="syntaxTree">The syntax tree that this visitor operates on.</param>
        public TypeDependencyEnumeratorSyntaxVisitor(ICompilation compilation, SyntaxTree syntaxTree)
        {
            if (compilation == null)
                throw new ArgumentNullException(nameof(compilation));

            if (syntaxTree == null)
                throw new ArgumentNullException(nameof(syntaxTree));

            _syntaxTree = syntaxTree;
            _resolver = new CSharpAstResolver(compilation, _syntaxTree);

            TypeDependencies = new List<TypeDependency>();
        }

        public override void VisitIdentifier(Identifier identifier)
        {
            foreach (var typeDependency in GetTypeDependencies(identifier))
                TypeDependencies.Add(typeDependency);
        }

        /// <summary>
        /// Finds type dependencies for a syntax tree node.
        /// </summary>
        /// <param name="node">A syntax tree node.</param>
        /// <returns>A list of type dependencies. Can be empty.</returns>
        private IEnumerable<TypeDependency> GetTypeDependencies(AstNode node)
        {
            // Determine the type that contains the current syntax node.
            var enclosingType = DetermineEnclosingType(node, _resolver);
            if (!IsCandidateForDependecyAnalysis(enclosingType))
                yield break;

            // Determine the type referenced by the symbol represented by the current syntax node.
            var referencedType = DetermineReferencedType(node, _resolver);
            if (IsCandidateForDependecyAnalysis(referencedType))
                yield return GetTypeDependency(enclosingType, referencedType, node);

            // If this is an extension method invocation then determine the type declaring the extension method.
            var declaringType = DetermineExtensionMethodDeclaringType(node, _resolver);
            if (IsCandidateForDependecyAnalysis(declaringType))
                yield return GetTypeDependency(enclosingType, declaringType, node);
        }

        /// <summary>
        /// Returns a value indicating whether the given type is subject of dependency analysis.
        /// </summary>
        /// <param name="typeSymbol">A type symbol.</param>
        /// <returns>True if the type symbol is subject of dependency analysis.</returns>
        private static bool IsCandidateForDependecyAnalysis(IType typeSymbol)
        {
            return typeSymbol?.Namespace != null
                && AnalyzedTypeKinds.Contains(typeSymbol.Kind);
        }

        /// <summary>
        /// Returns a type dependency object for the given types.
        /// </summary>
        /// <param name="fromType">The referring type.</param>
        /// <param name="toType">The referenced type.</param>
        /// <param name="node">The syntax node currently analyzed.</param>
        /// <returns>A type dependency object or null of could not create one.</returns>
        private TypeDependency GetTypeDependency(IType fromType, IType toType, AstNode node)
        {
            return new TypeDependency(
                fromType.Namespace, fromType.GetMetadataName(),
                toType.Namespace, toType.GetMetadataName(),
                GetSourceSegment(node, _syntaxTree.FileName));
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
            if (csharpInvocationResolveResult == null ||
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
        /// Creates a source segment object for an AST node.
        /// </summary>
        /// <param name="syntaxNode">The syntax node where the type dependency was detected.</param>
        /// <param name="filename">The full path of the source file.</param>
        /// <returns>A source segment object.</returns>
        private static SourceSegment GetSourceSegment(AstNode syntaxNode, string filename)
        {
            return new SourceSegment
            (
                syntaxNode.StartLocation.Line,
                syntaxNode.StartLocation.Column,
                syntaxNode.EndLocation.Line,
                syntaxNode.EndLocation.Column,
                syntaxNode.ToString(),
                filename
            );
        }
    }
}
