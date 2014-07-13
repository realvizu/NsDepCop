namespace A
{
    using B;

    class MyClass
    {
        void MyMethod()
        {
            MyOtherClass.MyOtherMethod();
        }            
    }
}

namespace B
{
    static class MyOtherClass
    {
        public static C.MyEnum MyOtherMethod()
        {
            return C.MyEnum.Value1;
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



