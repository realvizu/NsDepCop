namespace A
{
    using B;

    class Class1
    {
        public void Method1()
        {
            var a = new Class2<Class3>[10];
            a[1] = a[2];
        }
    }
}

namespace B
{
    class Class2<T>
    {
    }

    class Class3
    {
    }
}