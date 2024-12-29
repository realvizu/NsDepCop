using System;
using Microsoft.CodeAnalysis;

namespace Codartis.NsDepCop.Analysis
{
    [Serializable]
    public struct AssemblyDependency
    {
        public static AssemblyDependency Empty;

        public AssemblyIdentity FromAssembly { get; }

        public AssemblyIdentity ToAssembly { get; }

        public AssemblyDependency(AssemblyIdentity fromAssembly, AssemblyIdentity toAssembly)
        {
            FromAssembly = fromAssembly ?? throw new ArgumentNullException(nameof(fromAssembly));
            ToAssembly = toAssembly ?? throw new ArgumentNullException(nameof(toAssembly));
        }

        // TODO: should print source segment too
        public override string ToString() => $"{FromAssembly?.Name}->{ToAssembly?.Name}";

        // TODO: should consider version, culture info and public key token too
        public bool Equals(AssemblyDependency other)
        {
            return string.Equals(FromAssembly?.Name, other.FromAssembly?.Name) 
                && string.Equals(ToAssembly?.Name, other.ToAssembly?.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AssemblyDependency && Equals((AssemblyDependency) obj);
        }

        // TODO: should consider version, culture info and public key too
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (FromAssembly != null ? FromAssembly.Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (ToAssembly != null ? ToAssembly.Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(AssemblyDependency left, AssemblyDependency right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssemblyDependency left, AssemblyDependency right)
        {
            return !left.Equals(right);
        }
    }
}
