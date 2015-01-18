using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareApproach.TestingExtensions;
using System;
using System.Linq;
using Codartis.NsDepCop.Core.Common;

namespace Codartis.NsDepCop.Core.Test.Common
{
    [TestClass]
    public class NamespaceSpecificationTests
    {
        [TestMethod]
        public void Create()
        {
            new NamespaceSpecification("A.B.*").ToString().ShouldEqual("A.B.*");
        }

        [TestMethod]
        public void CreateGlobalNamespace_RoslynStyle()
        {
            new NamespaceSpecification("<global namespace>").ToString().ShouldEqual(".");
        }

        [TestMethod]
        public void CreateGlobalNamespace_NRefactoryStyle()
        {
            new NamespaceSpecification("").ToString().ShouldEqual(".");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateWithNull()
        {
            new NamespaceSpecification(null);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CreateInvalid()
        {
            new NamespaceSpecification("..");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void CreateInvalidUseOfAny()
        {
            new NamespaceSpecification("*.*");
        }

        [TestMethod]
        public void UntypedEquals_WithSameObject()
        {
            var ns = new NamespaceSpecification("A");
            ns.Equals((object)ns).ShouldBeTrue();
        }

        [TestMethod]
        public void UntypedEquals_WithDifferentType()
        {
            new NamespaceSpecification("A").Equals(1).ShouldBeFalse();
        }

        [TestMethod]
        public void UntypedEquals_WithNull()
        {
            new NamespaceSpecification("A").Equals(null).ShouldBeFalse();
        }

        [TestMethod]
        public void UntypedEquals_WithEqualObject()
        {
            new NamespaceSpecification("A").Equals((object)new NamespaceSpecification("A")).ShouldBeTrue();
        }

        [TestMethod]
        public void TypedEquals_WithSameObject()
        {
            var ns = new NamespaceSpecification("A");
            ns.Equals(ns).ShouldBeTrue();
        }

        [TestMethod]
        public void TypedEquals_WithNull()
        {
            new NamespaceSpecification("A").Equals((NamespaceSpecification)null).ShouldBeFalse();
        }

        [TestMethod]
        public void TypedEquals_WithEqualObject()
        {
            new NamespaceSpecification("A").Equals(new NamespaceSpecification("A")).ShouldBeTrue();
        }

        [TestMethod]
        public void EqualsOperator()
        {
            (new NamespaceSpecification("A") == new NamespaceSpecification("A")).ShouldBeTrue();
        }

        [TestMethod]
        public void UnequalsOperator()
        {
            (new NamespaceSpecification("A") != new NamespaceSpecification("B")).ShouldBeTrue();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetContainingNamespaceSpecifications_NullArgument()
        {
            NamespaceSpecification.GetContainingNamespaceSpecifications(null).ToList();
        }

        [TestMethod]
        public void GetContainingNamespaceSpecifications_GlobalNamespace()
        {
            var results = NamespaceSpecification.GetContainingNamespaceSpecifications(".");
            var expectedElements = new[]
            {
                new NamespaceSpecification("*"),
                new NamespaceSpecification(".")
            };
            expectedElements.Count().ShouldEqual(results.Count());
            expectedElements.All(i => results.Contains(i)).ShouldBeTrue();
        }

        [TestMethod]
        public void GetContainingNamespaceSpecifications_SingleTagNamespace()
        {
            var results = NamespaceSpecification.GetContainingNamespaceSpecifications("A");
            var expectedElements = new[]
            {
                new NamespaceSpecification("*"),
                new NamespaceSpecification("A"),
                new NamespaceSpecification("A.*"),
            };
            expectedElements.Count().ShouldEqual(results.Count());
            expectedElements.All(i => results.Contains(i)).ShouldBeTrue();
        }

        [TestMethod]
        public void GetContainingNamespaceSpecifications_MultiTagNamespace()
        {
            var results = NamespaceSpecification.GetContainingNamespaceSpecifications("A.B");
            var expectedElements = new[]
            {
                new NamespaceSpecification("*"),
                new NamespaceSpecification("A.B"),
                new NamespaceSpecification("A.*"),
                new NamespaceSpecification("A.B.*"),
            };
            expectedElements.Count().ShouldEqual(results.Count());
            expectedElements.All(i => results.Contains(i)).ShouldBeTrue();
        }
    }
}
