namespace A
{
    using B;

    unsafe class Class1
    {
        private C.Class3* e;

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

    unsafe class Class2
    {
        public static Class3* Method2() { return null; }
        public static Class3** Method3() { return null; }
    }
}

namespace C
{
    struct Class3
    {
    }
}
