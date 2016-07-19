using System;

namespace Codartis.NsDepCop.Core.Interface
{
    /// <summary>
    /// Defines the properties of an issue that can be reported by the tool.
    /// </summary>
    public class IssueDescriptor
    {
        /// <summary>
        /// The issue's short, unique identifier.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// The default severity of the issue.
        /// </summary>
        public IssueKind DefaultKind { get; private set; }

        /// <summary>
        /// A static description of the issue.
        /// </summary>
        public string StaticDescription { get; private set; }

        public IssueDescriptor(string id, IssueKind defaultKind, string staticDescription)
        {
            Id = id;
            DefaultKind = defaultKind;
            StaticDescription = staticDescription;
        }
    }

    /// <summary>
    /// Defines the properties of an issue that can be reported by the tool and has a dynamic description.
    /// </summary>
    /// <typeparam name="TIssueSubject">The type of the reported issue's subject.</typeparam>
    public class IssueDescriptor<TIssueSubject> : IssueDescriptor
    {
        /// <summary>
        /// A delegate that creates a description from an issue subject object.
        /// </summary>
        public Func<TIssueSubject, string> DescriptionFormatterDelegate { get; }

        public IssueDescriptor(string id, IssueKind defaultKind, string staticDescription, Func<TIssueSubject, string> descriptionFormatterDelegate = null)
            : base(id, defaultKind, staticDescription)
        {
            DescriptionFormatterDelegate = descriptionFormatterDelegate;
        }

        /// <summary>
        /// Returns the description of the issue dynamically created from a template string and an issue subject.
        /// </summary>
        /// <param name="issueSubject">The subject of the issue. Its properties can be used to create the dynamic description.</param>
        /// <returns>The description of the issue dynamically created from a template string and the supplied issue subject </returns>
        public string GetDynamicDescription(TIssueSubject issueSubject) => DescriptionFormatterDelegate?.Invoke(issueSubject);
    }
}
