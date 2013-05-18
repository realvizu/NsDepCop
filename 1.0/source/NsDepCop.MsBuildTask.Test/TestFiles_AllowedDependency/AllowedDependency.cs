namespace A
{
    public class NoIssue
    {
        private B.MyEnum field;
    }
}

namespace B
{
    public enum MyEnum
    { }
}
