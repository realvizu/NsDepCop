using System;
using System.Collections.Generic;
using System.Linq;

namespace Codartis.NsDepCop.Core.Interface.Config
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

        public override string ToString() => this.Any() ? $"{{{string.Join(",", this)}}}" : "{}";
    }
}
