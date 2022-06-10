using System.Xml.Linq;

namespace Codartis.NsDepCop.Test.Implementation.Config
{
    public class XmlFileConfigTestBase : FileBasedTestsBase
    {
        protected static void CreateConfigFile(string path, string isEnabledString, int inheritanceDepth = 0, int? maxIssueCount = null)
        {
            var document = XDocument.Parse(
                $"<NsDepCopConfig InheritanceDepth='{inheritanceDepth}' IsEnabled='{isEnabledString}' {GetMaxIssueCountAttributeString(maxIssueCount)} />");
            document.Save(path);
        }

        private static string GetMaxIssueCountAttributeString(int? maxIssueCount)
        {
            return maxIssueCount == null ? string.Empty : $" MaxIssueCount='{maxIssueCount}' ";
        }

        protected static string GetAttribute(string path, string attributeName)
        {
            var document = LoadXDocument(path);
            return document.Root!.Attribute(attributeName)?.Value;
        }


        protected static void SetAttribute(string path, string attributeName, string value)
        {
            var document = LoadXDocument(path);
            var xAttribute = document.Root!.Attribute(attributeName);

            if (xAttribute == null)
                document.Root.Add(new XAttribute(attributeName, value));
            else
                xAttribute.SetValue(value);

            document.Save(path);
        }

        protected static void RemoveAttribute(string path, string attributeName)
        {
            var document = LoadXDocument(path);
            document.Root!.Attribute(attributeName)?.Remove();
            document.Save(path);
        }

        private static XDocument LoadXDocument(string path)
        {
            return XDocument.Load(path, LoadOptions.SetLineInfo | LoadOptions.PreserveWhitespace);
        }
    }
}