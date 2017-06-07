using System;
using A;
using B;
using C;

namespace A
{
    public class Class1
    {
        public void Method()
        {
            // deconstructing declaration
            (var a, var b) = Method2();

            // deconstructing declaration with discard
            (_, var c) = Method2();

            // deconstructing declaration with var outside
            var (d, e) = Method2();

            // deconstructing assignment
            Class1 f;
            Class2 g;
            (f, g) = Method2();
        }

        public Deconstructable Method2() => throw new NotImplementedException();
    }
}

namespace B
{
    public class Deconstructable
    {
        public Class1 Class1 { get; set; }
        public Class2 Class2 { get; set; }

        public void Deconstruct(out Class1 class1, out Class2 class2)
        {
            class1 = Class1;
            class2 = Class2;
        }
    }
}

namespace C
{
    public class Class2
    {
    }
}