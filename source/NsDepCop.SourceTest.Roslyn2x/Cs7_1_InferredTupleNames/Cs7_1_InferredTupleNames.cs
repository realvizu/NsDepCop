using System;

namespace A
{
    using B;

    public class Class1
    {
        // https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.1/infer-tuple-names.md

        public void M1(Class2 p1)
        {
            var tuple = (p1.F1, p1.F2);

            var f1 = tuple.F1;
            var f2 = tuple.F2;
        }
    }
}

namespace B
{
    public class Class2
    {
        public int F1;
        public int F2;
    }
}