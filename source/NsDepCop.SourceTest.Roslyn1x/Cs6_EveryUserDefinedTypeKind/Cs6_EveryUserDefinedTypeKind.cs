namespace A
{
    using B;

    public class Foo
    {
        public MyClass X1;
        public IMyInterface X2;
        public MyStruct X3;
        public MyEnum X4;
        public MyDelegate X5;
    }
}

namespace B
{
    public class MyClass { }
    public interface IMyInterface { }
    public struct MyStruct { }
    public enum MyEnum { }
    public delegate void MyDelegate();
}
