using System;

namespace A
{
    using B;

    public class Class1
    {
        // https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.1/target-typed-default.md

        public Class2 M1(Class2 p1 = default)
        {
            Class2 x = default;
            var a = new[] {default, x};
            return default;
        }
    }
}

namespace B
{
    public class Class2
    {
    }
}