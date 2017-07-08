namespace A
{
    public class Class1
    {
        private B.DisallowedClass _disallowed1;

        private C.AllowedClass _allowed1;
        private C.DisallowedClass _disallowed2;

        private D.AllowedClass _allowed2;
        private D.DisallowedClass _disallowed3;
    }
}

namespace B
{
    public class DisallowedClass { }
}

namespace C
{
    public class AllowedClass { }
    public class DisallowedClass { }
}

namespace D
{
    public class AllowedClass { }
    public class DisallowedClass { }
}