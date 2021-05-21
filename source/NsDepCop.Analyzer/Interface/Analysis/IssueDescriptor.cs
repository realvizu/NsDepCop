using Codartis.NsDepCop.Interface.Config;

namespace Codartis.NsDepCop.Interface.Analysis
{
    /// <summary>
    /// Defines the properties of an issue that can be reported by the tool.
    /// </summary>
    public class IssueDescriptor
    {
        /// <summary>
        /// The issue's short, unique identifier.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The default severity of the issue.
        /// </summary>
        public IssueKind DefaultKind { get; }

        /// <summary>
        /// A title of the issue.
        /// </summary>
        public string Title { get; }

        public IssueDescriptor(string id, IssueKind defaultKind, string title)
        {
            Id = id;
            DefaultKind = defaultKind;
            Title = title;
        }
    }
}
