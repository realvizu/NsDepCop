namespace A
{
    using B;

    class Class1
    {
        void Method1()
        {
            Method2();
            new Class2().Method3();
            Class2.Method4();
            Class2.Method5();
        }

        C.Class3 Method2() { return null; }
    }
}

namespace B
{
    using C;

    class Class2
    {
        public Class3 Method3()
        {
            return null;
        }

        public static Class3 Method4()
        {
            return null;
        }

        public static Class4<Class3> Method5()
        {
            return null;
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



