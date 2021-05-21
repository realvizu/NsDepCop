using System;

namespace A
{
    using B;

    public class Class1
    {
        // Tuple return type
        public (Class1, Class2) Method1()
        {
            // Tuple literal
            return (new Class1(), new Class2());
        }

        // Named tuple elements
        public (Class1 class1, Class2 class2) Method2()
        {
            // Named tuple elements in tuple literal
            return (class1: new Class1(), class2: new Class2());
        }

        public void Method3()
        {
            // Get tuple value
            var a = Method2();
            // Access tuple element by defalt name
            a.Item2 = null;
            // Access tuple element by name
            a.class2 = null;
        }
    }
}

namespace B
{
    public class Class2
    {
    }
}