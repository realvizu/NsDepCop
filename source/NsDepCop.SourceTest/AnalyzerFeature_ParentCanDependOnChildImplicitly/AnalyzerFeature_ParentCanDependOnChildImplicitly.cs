public class NoIssue
{
    private A.MyEnum field1;
    private A.B.MyGlobalEnum field2;
}

namespace A
{
    public enum MyEnum
    { }
}

namespace A.B
{
    public enum MyGlobalEnum
    {
    }
}