namespace A
{
    using B;

    class MyClass
    {
        // Generic type is not allowed
        private MyGenericClass<MyClass2> e1;

        // Generic type and some type arguments are not allowed
        private MyGenericClass2<MyClass3, MyClass2, MyClass3> e2;

        // Generic type, nested generic type and type argument are not allowed
        private MyGenericClass<MyGenericClass<MyClass3>> e3;
    }

    class MyClass2 { }
}

namespace B
{
    class MyGenericClass<T1> { }

    class MyGenericClass2<T1, T2, T3> { }

    class MyClass3 { }
}
