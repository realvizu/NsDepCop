using Xunit;

namespace Codartis.NsDepCop.SourceTest
{
    /// <summary>
    /// These tests execute a DependencyAnalyzer with source files and a config and assert invalid dependencies.
    /// The name of the source file and its containing folder is the same as the name of the test.
    /// </summary>
    public class SourceTests
    {
        [Fact]
        public void AliasQualifiedName()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 25, 31)
                .Execute();
        }

        [Fact]
        public void ArrayType()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 19, 25)
                .ExpectInvalidSegment(11, 20, 27)
                .ExpectInvalidSegment(12, 20, 27)
                .Execute();
        }

        [Fact]
        public void QualifiedName()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(5, 19, 25)
                .Execute();
        }

        [Fact]
        public void InvocationExpression()
        {
            SourceTestSpecification.Create()

                // Class3
                .ExpectInvalidSegment(9, 13, 20)
                // Class3
                .ExpectInvalidSegment(10, 26, 33)
                // Class3
                .ExpectInvalidSegment(11, 20, 27)
                // Class4<Class3>
                .ExpectInvalidSegment(12, 20, 27)
                .ExpectInvalidSegment(12, 20, 27)
                // Class3
                .ExpectInvalidSegment(15, 11, 17)

                .Execute();
        }

        [Fact]
        public void InvocationWithTypeArg()
        {
            SourceTestSpecification.Create()

                // Class3
                .ExpectInvalidSegment(10, 28, 34)

                // Class4<Class3>
                .ExpectInvalidSegment(11, 28, 42)
                .ExpectInvalidSegment(11, 35, 41)

                .Execute();
        }

        [Fact]
        public void MemberAccessExpression()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(9, 37, 47)
                .Execute();
        }

        [Fact]
        public void GenericName()
        {
            SourceTestSpecification.Create()

                // MyGenericClass<MyClass2>
                .ExpectInvalidSegment(8, 17, 41)

                // MyGenericClass2<MyClass3, MyClass2, MyClass3>
                .ExpectInvalidSegment(11, 17, 62)
                // MyClass3
                .ExpectInvalidSegment(11, 33, 41)
                // MyClass3
                .ExpectInvalidSegment(11, 53, 61)

                // MyGenericClass<MyGenericClass<MyClass3>>
                .ExpectInvalidSegment(14, 17, 57)
                // MyGenericClass<MyClass3>
                .ExpectInvalidSegment(14, 32, 56)
                // MyClass3
                .ExpectInvalidSegment(14, 47, 55)

                .Execute();
        }

        [Fact]
        public void GenericTypeArgument()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 32, 40)
                .ExpectInvalidSegment(8, 36, 44)
                .Execute();
        }

        [Fact]
        public void NestedType()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 17, 25)
                .ExpectInvalidSegment(7, 26, 36)
                .ExpectInvalidSegment(8, 19, 27)
                .ExpectInvalidSegment(8, 28, 38)
                .Execute();
        }

        [Fact]
        public void PointerType()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 19, 25)
                .ExpectInvalidSegment(11, 20, 27)
                .ExpectInvalidSegment(12, 20, 27)
                .Execute();
        }

        [Fact]
        public void VeryComplexType()
        {
            SourceTestSpecification.Create()

                // Class4<Class3[], Class4<Class3*[], Class3[][]>>
                .ExpectInvalidSegment(10, 13, 60)
                .ExpectInvalidSegment(10, 20, 26)
                .ExpectInvalidSegment(10, 30, 59)
                .ExpectInvalidSegment(10, 37, 43)
                .ExpectInvalidSegment(10, 48, 54)

                // Method2 return value
                .ExpectInvalidSegment(10, 72, 79)
                .ExpectInvalidSegment(10, 72, 79)
                .ExpectInvalidSegment(10, 72, 79)
                .ExpectInvalidSegment(10, 72, 79)
                .ExpectInvalidSegment(10, 72, 79)

                .Execute();
        }

        [Fact]
        public void NullableType()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 17, 25)
                .Execute();
        }

        [Fact]
        public void EveryUserDefinedTypeKind()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 17, 24)
                .ExpectInvalidSegment(8, 17, 29)
                .ExpectInvalidSegment(9, 17, 25)
                .ExpectInvalidSegment(10, 17, 23)
                .ExpectInvalidSegment(11, 17, 27)
                .Execute();
        }

        [Fact]
        public void ExtensionMethodInvocation()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(9, 27, 44)
                .ExpectInvalidSegment(10, 27, 51)
                .Execute();
        }

        [Fact]
        public void ObjectCreationExpression()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(9, 17, 29)
                .Execute();
        }

        [Fact]
        public void Var()
        {
            SourceTestSpecification.Create()
                .ExpectInvalidSegment(7, 13, 16)
                .ExpectInvalidSegment(7, 23, 29)
                .ExpectInvalidSegment(7, 30, 40)
                .Execute();
        }

        [Fact]
        public void VarWithConstructedGenericType()
        {
            SourceTestSpecification.Create()

                // var: ClassB`2, EnumB, EnumB
                .ExpectInvalidSegment(8, 13, 16)
                .ExpectInvalidSegment(8, 13, 16)
                .ExpectInvalidSegment(8, 13, 16)

                // ClassB<B.EnumB, EnumA, B.EnumB>
                .ExpectInvalidSegment(8, 23, 54)
                // EnumB
                .ExpectInvalidSegment(8, 32, 37)
                // EnumB
                .ExpectInvalidSegment(8, 48, 53)

                // Instance: ClassB`2, EnumB, EnumB
                .ExpectInvalidSegment(8, 55, 63)
                .ExpectInvalidSegment(8, 55, 63)
                .ExpectInvalidSegment(8, 55, 63)

                .Execute();
        }

        [Fact]
        public void Attributes()
        {
            SourceTestSpecification.Create()

                // assembly and module attributes are not analyzed because there's no enclosing type
                //// [assembly: A.AllowedAttributeWithTypeArg(typeof(B.ForbiddenType))]
                //CreateLogEntryParameters(sourceFileName, 3, 14, 3, 41),
                //CreateLogEntryParameters(sourceFileName, 3, 51, 3, 64),
                //// [module: A.AllowedAttributeWithTypeArg(typeof(B.ForbiddenType))]
                //CreateLogEntryParameters(sourceFileName, 4, 12, 4, 39),
                //CreateLogEntryParameters(sourceFileName, 4, 49, 4, 62),
                //// [assembly: B.ForbiddenAttribute("foo")]
                //CreateLogEntryParameters(sourceFileName, 5, 14, 5, 32),
                //// [module: B.Forbidden("foo")]
                //CreateLogEntryParameters(sourceFileName, 6, 12, 6, 21),

                // [Forbidden on class
                .ExpectInvalidSegment(20, 6, 15)

                // class attribute type parameter
                .ExpectInvalidSegment(26, 41, 54)

                // field attribute type parameter
                .ExpectInvalidSegment(34, 45, 58)

                // enum value attribute type parameter
                .ExpectInvalidSegment(43, 45, 58)

                .Execute();
        }

        [Fact]
        public void Delegates()
        {
            SourceTestSpecification.Create()

                // delegate Class1<Class2> Delegate1(Class2 c);
                .ExpectInvalidSegment(5, 14, 28)
                .ExpectInvalidSegment(5, 21, 27)
                .ExpectInvalidSegment(5, 39, 45)

                .Execute();
        }

        [Fact]
        public void ElementAccess()
        {
            SourceTestSpecification.Create()

                // var a = new Class2<Class3>[10];
                .ExpectInvalidSegment(9, 13, 16)
                .ExpectInvalidSegment(9, 13, 16)
                .ExpectInvalidSegment(9, 25, 39)
                .ExpectInvalidSegment(9, 32, 38)

                // a[1] = a[2];
                .ExpectInvalidSegment(10, 13, 14)
                .ExpectInvalidSegment(10, 13, 14)
                .ExpectInvalidSegment(10, 20, 21)
                .ExpectInvalidSegment(10, 20, 21)

                .Execute();
        }

        [Fact]
        public void Cs7_Out()
        {
            SourceTestSpecification.Create()
                // out MyEnum x -- this is a variable declaration so only the type name is checked and not the variable.
                .ExpectInvalidSegment(11, 45, 51)
                .Execute();
        }

        [Fact]
        public void Cs7_Tuples()
        {
            SourceTestSpecification.Create()

                // public (Class1, Class2) Method1()
                .ExpectInvalidSegment(10, 25, 31)
                // return (new Class1(), new Class2());
                .ExpectInvalidSegment(13, 39, 45)

                // public (Class1 class1, Class2 class2) Method2()
                .ExpectInvalidSegment(17, 32, 38)
                // return (class1: new Class1(), class2: new Class2());
                .ExpectInvalidSegment(20, 55, 61)

                // var a = Method2() | var -> Class2 | Method2 -> Class2
                .ExpectInvalidSegment(26, 13, 16)
                .ExpectInvalidSegment(26, 21, 28)

                // a.Item2 = null; | a -> Class2 | Item2 -> Class2
                .ExpectInvalidSegment(28, 13, 14)
                .ExpectInvalidSegment(28, 15, 20)

                // a.class4 = null; | a -> Class2 | class4 -> Class2
                .ExpectInvalidSegment(30, 13, 14)
                .ExpectInvalidSegment(30, 15, 21)

                .Execute();
        }

        [Fact]
        public void Cs7_Deconstruction()
        {
            SourceTestSpecification.Create()

                // (var a, var b) = Method2();
                .ExpectInvalidSegment(13, 21, 24)
                
                // (_, var c) = Method2();
                .ExpectInvalidSegment(16, 17, 20)

                // var (d, e) = Method2();
                .ExpectInvalidSegment(19, 13, 16)

                // Class2 g;
                .ExpectInvalidSegment(23, 13, 19)
                // (f, g) = Method2();
                .ExpectInvalidSegment(24, 17, 18)

                .Execute();
        }

        [Fact]
        public void Cs7_IsExpressionWithPattern()
        {
            SourceTestSpecification.Create()

                // Class2 o
                .ExpectInvalidSegment(7, 28, 34)

                // o is Class3 class3 (type pattern)
                .ExpectInvalidSegment(9, 17, 18)
                .ExpectInvalidSegment(9, 22, 28)

                // o is var class2 (var pattern)
                .ExpectInvalidSegment(11, 17, 18)
                .ExpectInvalidSegment(11, 22, 25)
                
                .Execute();
        }

        [Fact]
        public void Cs7_SwitchWithPattern()
        {
            SourceTestSpecification.Create()

                // case Class2 class2 when class2 != null:
                .ExpectInvalidSegment(11, 22, 28)
                .ExpectInvalidSegment(11, 41, 47)

                .Execute();
        }

        [Fact]
        public void Cs7_LocalFunction()
        {
            SourceTestSpecification.Create()

                // Class2 LocalFunction(Class2 class2)
                .ExpectInvalidSegment(9, 13, 19)
                .ExpectInvalidSegment(9, 34, 40)

                .Execute();
        }

        [Fact]
        public void Cs7_ThrowExpression()
        {
            SourceTestSpecification.Create()

                // throw new MyException();
                .ExpectInvalidSegment(9, 43, 54)

                .Execute();
        }
    }
}
