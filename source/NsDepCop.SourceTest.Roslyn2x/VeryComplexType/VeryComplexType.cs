namespace A
{
    using B;
    using C;

    unsafe class Class1
    {
        private void Method1()
        {
            Class4<Class3[], Class4<Class3*[], Class3[][]>> a = Class2.Method2();
        }
    }
}

namespace B
{
    using C;

    unsafe class Class2
    {
        public static Class4<Class3[], Class4<Class3*[], Class3[][]>> Method2() { return null; }
    }
}

namespace C
{
    struct Class3
    {
    }

    class Class4<T1, T2>
    {
    }
}
