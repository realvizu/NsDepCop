namespace A
{
    using B;

    class Foo
    {
        private MyClass x1;
        private IMyInterface x2;
        private MyStruct x3;
        private MyEnum x4;
        private MyDelegate x5;
    }
}

namespace B
{
    class MyClass { }
    interface IMyInterface { }
    struct MyStruct { }
    enum MyEnum { }
    delegate void MyDelegate();
}
