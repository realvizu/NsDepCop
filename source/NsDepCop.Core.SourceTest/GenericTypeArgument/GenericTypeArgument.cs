namespace A
{
    using B;

    class MyClass
    {
        private MyGenericClass<MyClass2> e1;
        private A.MyGenericClass<B.MyClass2> e2;
    }

    class MyGenericClass<T1> { }
}

namespace B
{
    class MyClass2 { }
}
