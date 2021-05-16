using System;

namespace A
{
    using B;

    public class Class1
    {
        // https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.2/non-trailing-named-arguments.md

        public void M1(Class2 p1, Class2 p2)
        {
            M1(p1: p1, p2);
        }
    }
}

namespace B
{
    public class Class2
    {
    }
}