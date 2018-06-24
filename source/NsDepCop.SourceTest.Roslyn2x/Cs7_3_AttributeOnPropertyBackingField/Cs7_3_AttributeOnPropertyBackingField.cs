using System;

namespace A
{
    [Serializable]
    public class Class1
    {
        [field: NonSerialized]
        public Class1 P1 { get; set; }
    }
}
