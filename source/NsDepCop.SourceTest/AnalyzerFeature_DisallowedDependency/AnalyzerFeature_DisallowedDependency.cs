namespace A
{
    public class NoIssue
    {
        private B.MyEnum field1;
        private MyGlobalEnum field2;
        private System.Type field3;
    }
}

namespace B
{
    public enum MyEnum
    { }
}

public enum MyGlobalEnum
{ }