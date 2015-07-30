namespace A
{
    using B;
    using C;

    class MyClass
    {
        void MyMethod()
        {
            MyOtherMethod();
            MyOtherClass.MyOtherStaticMethod();
            new MyOtherClass().MyOtherMethod();
        }
        
        MyEnum MyOtherMethod() { return null; }
    }
}

namespace B
{
    class MyOtherClass
    {
        public C.MyEnum MyOtherMethod()
        {
            return C.MyEnum.Value1;
        }

        public static C.MyEnum MyOtherStaticMethod()
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



