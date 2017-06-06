using System;

namespace A
{
    using B;

    public class Class1
    {
        public void Method()
        {
            Enum.TryParse("EnumValue1", out MyEnum x);
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