namespace A
{
    using B;

    class MyClass
    {
        void MyMethod()
        {
            new MyClass().MyExtensionMethod();
            new MyClass().MyGenericExtensionMethod();
        }
    }
}

namespace B
{
    using A;

    static class MyClassExtensions
    {
        public static void MyExtensionMethod(this MyClass myClass)
        {
        }

        public static void MyGenericExtensionMethod<T>(this T t)
        {
        }   
    }
}
