namespace A
{
    using B;

    class MyClass
    {
        // Generic type is not allowed
        private MyGenericClass<MyClass2> e1;

        // Generic type and type argument are not allowed
        private MyGenericClass<MyClass3> e2;
    }

    class MyClass2 { }
}

namespace B
{
    class MyGenericClass<T1> { }

    class MyClass3 { }
}
