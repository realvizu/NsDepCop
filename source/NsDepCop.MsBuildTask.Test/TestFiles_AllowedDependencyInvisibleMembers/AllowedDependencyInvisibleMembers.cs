namespace A
{
    public class MyClass
    {
        private B.VisibleType field1;
        private B.InvisibleType field2;
        private B.OnlyGenericIsVisibleType<int> field3;
        private B.OnlyGenericIsVisibleType field4;
        private B.InvisibleGenericType<int> field5;
    }
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
