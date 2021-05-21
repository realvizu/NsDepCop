namespace A
{
    using B;

    public class Class1
    {
        public void Method(object o)
        {
            switch (o)
            {
                case Class2 class2 when class2 != null:
                    break;
            }
        }
    }
}

namespace B
{
    public class Class2
    { }
}