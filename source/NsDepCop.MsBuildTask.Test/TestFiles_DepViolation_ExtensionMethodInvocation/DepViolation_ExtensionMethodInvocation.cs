namespace A
{
    using B;

    class MyClass
    {
        void MyMethod()
        {
            new MyClass().MyExtensionMethod();
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
    }
}
