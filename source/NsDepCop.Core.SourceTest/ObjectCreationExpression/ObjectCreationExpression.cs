namespace A
{
    using B;

    class MyClass
    {
        void MyMethod()
        {
            new MyOtherClass();
        }
    }
}

namespace B
{
    class MyOtherClass { }
}


