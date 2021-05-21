namespace A.B
{
    public class NoIssue
    {
        private A.MyEnum field1;
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