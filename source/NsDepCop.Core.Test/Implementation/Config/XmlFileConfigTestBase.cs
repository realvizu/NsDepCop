using System.Xml.Linq;
using Codartis.NsDepCop.TestUtil;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    public class XmlFileConfigTestBase : FileBasedTestsBase
    {
        protected static void CreateConfigFile(string path, string isEnabledString, int inheritanceDepth = 0)
        {
            var document = XDocument.Parse($"<NsDepCopConfig InheritanceDepth='{inheritanceDepth}' IsEnabled='{isEnabledString}'/>");
            document.Save(path);
        }

        protected static void SetAttribute(string path, string attributeName, string value)
        {
            var document = XDocument.Load(path);
            document.Root.Attribute(attributeName).SetValue(value);
            document.Save(path);
        }
    }
}