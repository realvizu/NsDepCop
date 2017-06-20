using System;

namespace A
{
    using B;

    public class Class1
    {
        public void Method() => throw new MyException();
    }
}

namespace B
{
    public class MyException : Exception
    { }
}