namespace A
{
    public class MyClass
    {
        private B.VisibleType field1;
        private B.InvisibleType field2;
        private B.OnlyGenericIsVisibleType<MyEnum> field3;
        private B.OnlyGenericIsVisibleType field4;
        private B.InvisibleGenericType<MyEnum> field5;
    }

    public enum MyEnum { }
}

namespace B
{
    public enum VisibleType
    { }

    public enum InvisibleType
    { }

    public class OnlyGenericIsVisibleType<T>
    { }

    public class OnlyGenericIsVisibleType
    { }

    public class InvisibleGenericType<T>
    { }
}
