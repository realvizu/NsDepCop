namespace Codartis.NsDepCop.Core.Interface.Analysis
{
    /// <summary>
    /// Describes a dependency between two types.
    /// </summary>
    public struct TypeDependency
    {
        /// <summary>
        /// The namespace of the referencing type.
        /// </summary>
        public string FromNamespaceName { get; }

        /// <summary>
        /// The name of the referencing type.
        /// </summary>
        public string FromTypeName { get; }

        /// <summary>
        /// The namespace of the referenced type.
        /// </summary>
        public string ToNamespaceName { get; }

        /// <summary>
        /// The name of the referenced type.
        /// </summary>
        public string ToTypeName { get; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public TypeDependency(string fromNamespaceName, string fromTypeName, string toNamespaceName, string toTypeName)
        {
            FromNamespaceName = fromNamespaceName;
            FromTypeName = fromTypeName;
            ToNamespaceName = toNamespaceName;
            ToTypeName = toTypeName;
        }

        public override string ToString() 
            => $"{FromNamespaceName}.{FromTypeName}->{ToNamespaceName}.{ToTypeName}";

        public bool Equals(TypeDependency other)
        {
            return string.Equals(FromNamespaceName, other.FromNamespaceName) 
                && string.Equals(FromTypeName, other.FromTypeName) 
                && string.Equals(ToNamespaceName, other.ToNamespaceName) 
                && string.Equals(ToTypeName, other.ToTypeName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TypeDependency && Equals((TypeDependency) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FromNamespaceName != null ? FromNamespaceName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (FromTypeName != null ? FromTypeName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ToNamespaceName != null ? ToNamespaceName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ToTypeName != null ? ToTypeName.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(TypeDependency left, TypeDependency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TypeDependency left, TypeDependency right)
        {
            return !left.Equals(right);
        }
    }
}
