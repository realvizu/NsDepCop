using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Codartis.NsDepCop.Interface.Config
{
    /// <summary>
    /// A set of type names represented as strings (without namespace part).
    /// </summary>
    [Serializable]
    public class TypeNameSet : HashSet<string>
    {
        public TypeNameSet()
        {
        }

        public TypeNameSet(IEnumerable<string> collection)
            : base(collection)
        {
        }

        protected TypeNameSet(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        public override string ToString() => this.Any() ? $"{{{string.Join(",", this)}}}" : "{}";
    }
}
