namespace A
{
    using B;

    class MyClass
    {
        void MyMethod()
        {
            object o = MyOtherClass.MyProperty;
        }
    }
}

namespace B
{
    static class MyOtherClass
    {
        public static C.MyEnum MyProperty { get; set; }
    }
}

namespace C
{
    enum MyEnum
    {
        Value1
    }
}