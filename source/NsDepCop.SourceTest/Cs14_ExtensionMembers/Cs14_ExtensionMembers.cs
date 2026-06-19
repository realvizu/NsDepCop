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
        extension(MyClass myClass)
        {
            public void MyExtensionMethod()
            {
            }
        }

        extension<T>(T t)
        {
            public void MyGenericExtensionMethod()
            {
            }
        }
    }
}
