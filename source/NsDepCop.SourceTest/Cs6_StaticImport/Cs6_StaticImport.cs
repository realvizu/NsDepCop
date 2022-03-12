namespace B
{
    using static A.ClassA;

    public class ClassB
    {
        public void CallA()
        {
            IsCalledReturnsVoid();    // #60 call dependency itself is ignored
            IsCalledReturnsClassA();  // #60 produces warning based on return type (but not on the call itself)
        }
    }
}

namespace A
{
    public class ClassA
    {
        public static void IsCalledReturnsVoid() { _ = new ClassA();}
        public static ClassA IsCalledReturnsClassA() { return new ClassA(); }
    }
}