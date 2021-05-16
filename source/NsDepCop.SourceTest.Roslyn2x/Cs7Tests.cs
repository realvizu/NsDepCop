using Xunit;

namespace Codartis.NsDepCop.SourceTest
{
    /// <summary>
    /// Tests that the analyzer handles various C# 7.0 constructs correctly.
    /// </summary>
    /// <remarks>
    /// The name of the source file and its containing folder is the same as the name of the test.
    /// </remarks>
    public class Cs7Tests
    {
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