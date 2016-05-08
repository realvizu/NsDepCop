namespace A
{
    public class NoIssue
    {
        private A.MyEnum field;
    }

    public enum MyEnum
    { }

    public class OnlyTypeVisibleOutside
    { }
}