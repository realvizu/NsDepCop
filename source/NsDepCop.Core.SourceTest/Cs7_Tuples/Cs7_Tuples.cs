namespace A
{
    using B;

    public class Class1
    {
        public void Method()
        {
            (var a, var b) = Class2.Method2();
        }
    }
}

namespace B
{
    public static class Class2
    {
        public static (MyEnum1, MyEnum2) Method2()
        {
            return (MyEnum1.EnumValue1, MyEnum2.EnumValue2);
        }
    }

    public enum MyEnum1
    {
        EnumValue1
    }

    public enum MyEnum2
    {
        EnumValue2
    }
}