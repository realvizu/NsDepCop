using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Codartis.NsDepCop.MsBuildTask
{
    /// <summary>
    /// Indentifies an assembly with name, public key token and culture. No version info included.
    /// </summary>
    internal struct AssemblyIdentity : IEquatable<AssemblyIdentity>
    {
        public string Name { get; }
        public string PublicKeyToken { get; }
        public string Culture { get; }

        public AssemblyIdentity(string name, string publicKeyToken, string culture)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            PublicKeyToken = publicKeyToken ?? throw new ArgumentNullException(nameof(publicKeyToken));
            Culture = culture ?? throw new ArgumentNullException(nameof(culture));
        }

        public AssemblyIdentity(AssemblyName assemblyName)
            : this(assemblyName.Name, GetPublicKeyTokenAsString(assemblyName), GetCultureName(assemblyName))
        { }

        public bool Equals(AssemblyIdentity other)
        {
            return string.Equals(Name, other.Name)
                && string.Equals(PublicKeyToken, other.PublicKeyToken, StringComparison.OrdinalIgnoreCase)
                && string.Equals(Culture, other.Culture, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is AssemblyIdentity && Equals((AssemblyIdentity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ PublicKeyToken.GetHashCode();
                hashCode = (hashCode * 397) ^ Culture.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(AssemblyIdentity left, AssemblyIdentity right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AssemblyIdentity left, AssemblyIdentity right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => $"Name={Name}, PublicKeyToken={PublicKeyToken}, Culture={Culture}";

        private static string GetPublicKeyTokenAsString(AssemblyName assemblyName)
            => string.Concat(assemblyName.GetPublicKeyToken().Select(i => i.ToString("x2")));

        private static string GetCultureName(AssemblyName assemblyName)
        {
            var cultureInfo = assemblyName.CultureInfo;
            return Equals(cultureInfo, CultureInfo.InvariantCulture)
                ? "neutral"
                : cultureInfo.ToString();
        }
    }
}