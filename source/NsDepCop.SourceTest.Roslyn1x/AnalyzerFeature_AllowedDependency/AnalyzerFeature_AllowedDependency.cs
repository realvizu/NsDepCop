namespace A
{
    public class NoIssue
    {
        public B.MyEnum Field1;
        public MyGlobalEnum Field2;
        public System.Type Field3;
    }
}

namespace B
{
    public enum MyEnum
    { }
}

public enum MyGlobalEnum
{ }