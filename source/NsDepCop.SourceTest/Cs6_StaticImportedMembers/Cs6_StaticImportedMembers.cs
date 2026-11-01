using static A.ClassA;

namespace B
{
    public class ClassB
    {
        public byte[] ReadField()
        {
            return Fingerprint;
        }

        public int ReadConstant()
        {
            return Count;
        }

        public int ReadMemberOfField()
        {
            return Fingerprint.Length;
        }

        public int ReadProperty()
        {
            return Size;
        }

        public void SubscribeToEvent()
        {
            Changed += null;
        }

        public byte[] ReadQualifiedField()
        {
            return A.ClassA.Fingerprint;
        }

        public byte[] ReadLocal()
        {
            byte[] Fingerprint = new byte[0];
            return Fingerprint;
        }
    }
}

namespace A
{
    public class ClassA
    {
        public static readonly byte[] Fingerprint = new byte[0];
        public const int Count = 3;
        public static int Size { get; set; }
        public static event System.Action Changed;
        public static void RaiseChanged() { Changed?.Invoke(); }
    }
}
