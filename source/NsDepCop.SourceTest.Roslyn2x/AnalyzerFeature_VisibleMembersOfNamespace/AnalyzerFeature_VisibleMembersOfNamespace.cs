namespace A
{
    public class MyClass
    {
        private C.VisibleType field1;
        private C.InvisibleType field2;
        private C.OnlyGenericIsVisibleType<MyEnum> field3;
        private C.OnlyGenericIsVisibleType field4;
        private C.InvisibleGenericType<MyEnum> field5;
    }

    public enum MyEnum { }
}

namespace B
{
    public class MyClass
    {
        private C.VisibleType field1;
        private C.InvisibleType field2;
        private C.OnlyGenericIsVisibleType<MyEnum> field3;
        private C.OnlyGenericIsVisibleType field4;
        private C.InvisibleGenericType<MyEnum> field5;
    }

    public enum MyEnum { }
}

namespace C
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
