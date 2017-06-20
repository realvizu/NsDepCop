namespace A
{
    using B;

    class MyClass
    {
        private MyClass2.MySubclass e1;
        private B.MyClass2.MySubclass e2;
    }
}

namespace B
{
    class MyClass2 
    {
        public class MySubclass { }
    }
}
