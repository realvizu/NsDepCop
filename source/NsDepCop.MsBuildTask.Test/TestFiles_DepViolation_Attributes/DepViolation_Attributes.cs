using System;

namespace A
{
    using B;

    [AttributeUsage(AttributeTargets.All)]
    class AllowedAttributeWithTypeArg : Attribute
    {
        public AllowedAttributeWithTypeArg(Type t)
        {
        }
    }

    [Forbidden("Attributes defined in the foreign namespace detected ok")]
    class Foo2
    {
    }

    // Reference to forbidden type in arg to attribute on class. Detected OK.
    [AllowedAttributeWithTypeArg(typeof(ForbiddenType))]
    class Foo3
    {
    }

    class Foo4
    {
        // Reference to forbidden type in arg to attribute on class field. Detected OK.
        [AllowedAttributeWithTypeArg(typeof(ForbiddenType))]
        public int x;
    }

    enum Foo5
    {
        Value1,

        // Reference to forbidden type in arg to attribute on enum value. Not Detected!
        [AllowedAttributeWithTypeArg(typeof(ForbiddenType))]
        Value2
    }
}

namespace B
{
    class ForbiddenAttribute : Attribute
    {
        public ForbiddenAttribute(string s)
        { }
    }

    class ForbiddenType
    {
    }
}