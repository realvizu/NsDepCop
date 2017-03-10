using System.Xml.Linq;

namespace Codartis.NsDepCop.Core.Test.Implementation.Config
{
    public class XmlFileConfigTestBase : FileBasedTestsBase
    {
        protected static void CreateConfigFile(string path, string isEnabledString)
        {
            var document = XDocument.Parse($"<NsDepCopConfig IsEnabled='{isEnabledString}'/>");
            document.Save(path);
        }

        protected static void SetIsEnabled(string path, string isEnabledString)
        {
            var document = XDocument.Load(path);
            document.Root.Attribute("IsEnabled").SetValue(isEnabledString);
            document.Save(path);
        }
    }
}