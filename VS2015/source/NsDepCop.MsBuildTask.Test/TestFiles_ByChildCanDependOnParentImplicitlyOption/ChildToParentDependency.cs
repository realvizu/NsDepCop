namespace A.B
{
    public class NoIssue
    {
        private B.MyEnum field1;
        private MyGlobalEnum field2;
    }
}

namespace A
{
    public enum MyEnum
    { }
}

public enum MyGlobalEnum
{ }