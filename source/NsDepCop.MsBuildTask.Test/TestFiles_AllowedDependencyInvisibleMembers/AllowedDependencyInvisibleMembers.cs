namespace A
{
    public class AllowedSourceType1
    {
        private B.VisibleType1 field1;
        private B.VisibleType2 field2;
        private B.InvisibleType1 field3;
        private B.InvisibleType2 field4;
    }
}

namespace B
{
    public enum VisibleType1
    { }

    public class VisibleType2
    { }

    public enum InvisibleType1
    { }

    public class InvisibleType2
    { }
}
