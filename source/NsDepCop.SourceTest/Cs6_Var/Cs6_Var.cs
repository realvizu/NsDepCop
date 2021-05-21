namespace A
{
    public class Class1
    {
        public void Method()
        {
            var a = B.MyEnum.EnumValue1;
        }
    }
}

namespace B
{
    public enum MyEnum
    {
        EnumValue1
    }
}