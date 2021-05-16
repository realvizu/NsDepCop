namespace A
{
    using B;

    public class Foo
    {
        public MyClass1 X1;
        public IMyInterface1 X2;
        public MyStruct1 X3;
        public MyEnum1 X4;
        public MyDelegate1 X5;
    }
}

namespace B
{
    public class MyClass1 { }
    public interface IMyInterface1 { }
    public struct MyStruct1 { }
    public enum MyEnum1 { }
    public delegate void MyDelegate1();
}
