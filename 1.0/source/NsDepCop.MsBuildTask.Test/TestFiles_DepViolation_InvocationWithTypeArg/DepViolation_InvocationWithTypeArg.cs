namespace A
{
    using B;

    class MyClass
    {
        void MyMethod()
        {
            MyOtherClass.MyOtherMethod<C.MyEnum>();
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
    enum MyEnum
    {
        Value1
    }
}



