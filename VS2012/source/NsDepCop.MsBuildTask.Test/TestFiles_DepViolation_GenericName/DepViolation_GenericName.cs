namespace A
{
    using B;

    class MyClass
    {
        private MyGenericClass<MyClass2> e;
    }

    class MyClass2 { }
}

namespace B
{
    class MyGenericClass<T1> { }
}
