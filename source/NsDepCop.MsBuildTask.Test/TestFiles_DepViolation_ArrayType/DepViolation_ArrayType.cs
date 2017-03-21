namespace A
{
    using B;

    class Class1
    {
        private C.Class3[] e;

        private void Method1()
        {
            Class2.Method2();
            Class2.Method3();
        }
    }
}

namespace B
{
    using C;

    class Class2
    {
        public static Class3[] Method2() { return null; }
        public static Class3[][] Method3() { return null; }
    }
}

namespace C
{
    class Class3
    {
    }
}
