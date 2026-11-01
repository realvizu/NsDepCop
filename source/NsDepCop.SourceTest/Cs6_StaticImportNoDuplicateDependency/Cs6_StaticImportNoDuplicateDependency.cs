using static A.ClassA;
using static A.GenericA<int>;

namespace B
{
    public class ClassB
    {
        public object ReadField()
        {
            return Default;
        }

        public object CallMethod()
        {
            return Create();
        }

        public object ReadFieldOfGenericType()
        {
            return GenericDefault;
        }
    }
}

namespace A
{
    public class ClassA
    {
        public static readonly ClassA Default = new ClassA();
        public static ClassA Create() { return new ClassA(); }
    }

    public class GenericA<T>
    {
        public static readonly GenericA<T> GenericDefault = new GenericA<T>();
    }
}
