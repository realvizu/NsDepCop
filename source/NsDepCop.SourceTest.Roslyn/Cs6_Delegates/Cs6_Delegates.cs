namespace A
{
    using B;

    delegate Class1<Class2> Delegate1(Class2 c);
}

namespace B
{
    class Class1<T>
    {
    }

    class Class2
    {
    }
}