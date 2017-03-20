namespace A
{
    public class Class1
    {
        public void Method()
        {
            // var and Instance should have 2 dependency violations each (generic type MyClass and type argument MyEnum)
            var b = B.MyClass<B.MyEnum>.Instance;
        }
    }
}

namespace B
{
    public enum MyEnum
    {
    }

    public class MyClass<T>
    {
        public MyClass<T> Instance;
    }
}