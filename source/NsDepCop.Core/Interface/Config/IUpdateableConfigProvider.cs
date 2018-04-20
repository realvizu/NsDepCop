namespace Codartis.NsDepCop.Core.Interface.Config
{
    /// <summary>
    /// Provides config info and config state. Some config data can also be updated.
    /// </summary>
    public interface IUpdateableConfigProvider: IConfigProvider
    {
        /// <summary>
        /// Updates the MaxIssueCount config parameter to the given value and persists the changes.
        /// </summary>
        /// <param name="newValue">The new value for MaxIssueCount.</param>
        void UpdateMaxIssueCount(int newValue);
    }
}
