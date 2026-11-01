namespace A
{
    using B;

    class MyClass
    {
        void MyMethod()
        {
            new MyClass().MyExtensionMethod();
            new MyClass().MyGenericExtensionMethod();
            _ = new MyClass().MyExtensionProperty;
            _ = MyClass.MyStaticExtensionProperty;
            MyClass.MyStaticExtensionMethod();
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

            public int MyExtensionProperty => 1;

            public static int MyStaticExtensionProperty => 2;

            public static void MyStaticExtensionMethod()
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
