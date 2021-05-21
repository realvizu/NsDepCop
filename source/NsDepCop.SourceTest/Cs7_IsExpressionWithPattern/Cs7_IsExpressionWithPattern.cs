namespace A
{
    using B;

    public class Class1
    {
        public void Method(Class2 o)
        {
            if (o is Class3 class3) return;
        }
    }
}

namespace B
{
    public class Class2
    { }

    public class Class3 : Class2
    { }
}