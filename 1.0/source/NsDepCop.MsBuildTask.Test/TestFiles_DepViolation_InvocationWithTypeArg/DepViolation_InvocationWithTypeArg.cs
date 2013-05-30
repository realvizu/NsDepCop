namespace A
{
    using B;

    class MyClass
    {
        void MyMethod()
        {
            MyOtherClass.MyOtherMethod<C.MyClassC>(null);
        }
    }
}

namespace B
{
    static class MyOtherClass
    {
        public static void MyOtherMethod<T>(T t)
        {
        }
    }
}

namespace C
{
    class MyClassC
    {
    }
}



