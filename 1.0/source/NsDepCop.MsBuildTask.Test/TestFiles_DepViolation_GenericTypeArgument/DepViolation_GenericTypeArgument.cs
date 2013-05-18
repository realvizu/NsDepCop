namespace A
{
    using B;

    class MyClass
    {
        private MyGenericClass<MyClass2> e;
    }

    class MyGenericClass<T1> { }
}

namespace B
{
    class MyClass2 { }
}
