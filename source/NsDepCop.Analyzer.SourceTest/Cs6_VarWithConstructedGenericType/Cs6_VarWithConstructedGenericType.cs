namespace A
{
    public class ClassA
    {
        public void Method()
        {
            // var and Instance should have 3 dependency violations each (generic type ClassB and type arguments EnumB)
            var b = B.ClassB<B.EnumB, EnumA, B.EnumB>.Instance;
        }
    }

    public enum EnumA
    {
    }
}

namespace B
{
    public enum EnumB
    {
    }

    public class ClassB<T1, T2, T3>
    {
        public static ClassB<T1, T2, T3> Instance;
    }
}