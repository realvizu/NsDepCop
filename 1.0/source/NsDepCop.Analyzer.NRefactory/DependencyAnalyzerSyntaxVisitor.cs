using Codartis.NsDepCop.Core;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.CSharp.Resolver;
using ICSharpCode.NRefactory.TypeSystem;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Codartis.NsDepCop.Analyzer.NRefactory
{
    /// <summary>
    /// Traverses a syntax tree and collects dependency violations.
    /// </summary>
    internal class DependencyAnalyzerSyntaxVisitor : DepthFirstAstVisitor
    {
        /// <summary>
        /// Represents the project that the analysis is working on.
        /// </summary>
        private ICompilation _compilation;

        /// <summary>
        /// The syntax tree that this visitor operates on.
        /// </summary>
        private SyntaxTree _syntaxTree;

        /// <summary>
        /// The configuration of the tool. Containes the dependency rules.
        /// </summary>
        private NsDepCopConfig _config;

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
        public DependencyAnalyzerSyntaxVisitor(ICompilation compilation, SyntaxTree syntaxTree, NsDepCopConfig config)
        {
            if (compilation == null)
                throw new ArgumentNullException("compilation");

            if (syntaxTree == null)
                throw new ArgumentNullException("syntaxTree");

            if (config == null)
                throw new ArgumentNullException("config");

            _compilation = compilation;
            _syntaxTree = syntaxTree;
            _config = config;

            DependencyViolations = new List<DependencyViolation>();
        }

        /// <summary>
        /// Single identifiers (eg. MyClass) and first parts of multipart identifiers (eg. [A].MyClass)
        /// </summary>
        /// <param name="simpleType"></param>
        public override void VisitSimpleType(SimpleType simpleType)
        {
            CheckNode(simpleType);
            // Can have type argument child nodes hence the recursion is needed.
            base.VisitSimpleType(simpleType);
        }

        /// <summary>
        /// Multipart identifiers (eg. B.MyEnum). Also all sub-multipart identifiers (eg. [System.IO].File)
        /// </summary>
        /// <param name="memberType"></param>
        public override void VisitMemberType(MemberType memberType)
        {
            CheckNode(memberType);
            // Can have type argument child nodes hence the recursion is needed.
            base.VisitMemberType(memberType);
        }

        /// <summary>
        /// The first part of a multipart identifier in an expression. Eg. [MyClass].MyMethod or [MyMethod]()
        /// </summary>
        /// <param name="identifierExpression"></param>
        public override void VisitIdentifierExpression(IdentifierExpression identifierExpression)
        {
            CheckNode(identifierExpression);
            // It can have only an identifier child so no need to check children.
        }

        /// <summary>
        /// A multipart identifier in an expression. Eg. C.MyEnum.Value1 and [C.MyEnum].Value1
        /// </summary>
        /// <param name="memberReferenceExpression"></param>
        public override void VisitMemberReferenceExpression(MemberReferenceExpression memberReferenceExpression)
        {
            CheckNode(memberReferenceExpression);
            base.VisitMemberReferenceExpression(memberReferenceExpression);
        }

        /// <summary>
        /// Expression with an invocation. Eg. MyMethod(). 
        /// The child MyMethod is an IdentifierExpression with MethodGroup type.
        /// </summary>
        /// <param name="invocationExpression"></param>
        public override void VisitInvocationExpression(InvocationExpression invocationExpression)
        {
            CheckNode(invocationExpression);
            base.VisitInvocationExpression(invocationExpression);
        }

        // Not visited:
        //  VisitIdentifier: In NRefactory identifiers don't have type, only higher level constructs have.
        //  VisitPrimitiveType: Eg. void, object, int. Always references System, there's no value in showing them.
        //  VisitComposedType: Array (eg. MyClass[]), nullable (eg. MyStruct?) and pointer (eg. int*).
        //                     This node is not checked because its underlying type (child node) will be checked.
        //  VisitTypeReferenceExpression: It's a parent of a PrimitiveType in an expression. It's child will be checked.

        /// <summary>
        /// Performs the analysis on the given node and creates a dependency violation object if needed.
        /// </summary>
        /// <param name="node">A syntax node.</param>
        private void CheckNode(AstNode node)
        {
            var dependencyViolation = ProcessSyntaxNode(node, _compilation, _syntaxTree, _config);
            if (dependencyViolation != null && DependencyViolations.Count < _config.MaxIssueCount)
                DependencyViolations.Add(dependencyViolation);
        }

        /// <summary>
        /// Performs namespace dependency analysis for a syntax tree node.
        /// </summary>
        /// <param name="node">A syntax tree node.</param>
        /// <param name="compilation">The representation of the current project.</param>
        /// <param name="syntaxTree">The syntax tree that contains the given node.</param>
        /// <param name="config">Tool configuration info. Contains the allowed dependencies.</param>
        /// <returns>A DependencyViolation if an issue was found. Null if no problem.</returns>
        private static DependencyViolation ProcessSyntaxNode(
            AstNode node, ICompilation compilation, SyntaxTree syntaxTree, NsDepCopConfig config)
        {
            var resolver = new CSharpAstResolver(compilation, syntaxTree);

            // Determine the type of the symbol represented by the current syntax node.
            var referencedType = DetermineTypeOfAstNode(node, resolver);
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
            if (config.IsAllowedDependency(from, to))
                return null;

            // Create a result item for a dependency violation.
            return CreateDependencyViolation(node, new Dependency(from, to), enclosingType, referencedType, syntaxTree.FileName);
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
            if (resolveResult == null 
                || resolveResult.IsError 
                || resolveResult.Type == null)
                return null;

            if (resolveResult.Type.Kind == TypeKind.Class
                || resolveResult.Type.Kind == TypeKind.Interface
                || resolveResult.Type.Kind == TypeKind.Struct
                || resolveResult.Type.Kind == TypeKind.Enum
                || resolveResult.Type.Kind == TypeKind.Delegate)
                return resolveResult.Type;

            return null;
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
            var typeDeclarationSyntaxNode = node.Ancestors.Where(i => i is TypeDeclaration).FirstOrDefault();
            if (typeDeclarationSyntaxNode == null)
                return null;

            return DetermineTypeOfAstNode(typeDeclarationSyntaxNode, resolver);
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

            return new DependencyViolation(illegalDependency, referencingType.Name, referencedType.Name, sourceSegment);
        }
    }
}
