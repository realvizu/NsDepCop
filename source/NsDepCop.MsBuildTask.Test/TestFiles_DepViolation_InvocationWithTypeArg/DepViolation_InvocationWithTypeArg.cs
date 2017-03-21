namespace A
{
    using B;
    using C;

    class Class1
    {
        void Method1()
        {
            Class2.Method2<Class3>();
            Class2.Method2<Class4<Class3>>();
        }
    }
}

namespace B
{
    static class Class2
    {
        public static void Method2<T>()
        {
        }
    }
}

namespace C
{
    class Class3
    {
    }

    class Class4<T>
    {
    }
}
