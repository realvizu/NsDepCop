namespace Codartis.NsDepCop.Core
{
    public class IssueDescriptor
    {
        public string Id { get; private set; }
        public IssueKind DefaultKind { get; private set; }
        public string Description { get; private set; }
        public string MessageFormat { get; private set; }

        public IssueDescriptor(string id, IssueKind defaultKind, string description , string messageFormat = null)
        {
            Id = id;
            DefaultKind = defaultKind;
            Description = description;
            MessageFormat = messageFormat;
        }
    }
}
