namespace Codartis.NsDepCop.Analysis.Implementation
{
    public class IllegalTypeDependency
    {
        public TypeDependency TypeDependency { get; }
        
        public string[] AllowedMembers { get; }
    
        public IllegalTypeDependency(TypeDependency typeDependency, string[] allowedMembers)
        {
            TypeDependency = typeDependency;
            AllowedMembers = allowedMembers;
        }
    }
}