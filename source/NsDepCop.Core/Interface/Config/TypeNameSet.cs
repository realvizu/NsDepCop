using System.Collections.Generic;

namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// A set of type names represented as strings (without namespace part).
    /// </summary>
    public class TypeNameSet : HashSet<string>
    {
        public TypeNameSet()
        {
        }

        public TypeNameSet(IEnumerable<string> collection) 
            : base(collection)
        {
        }
    }
}
