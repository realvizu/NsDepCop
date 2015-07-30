using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace Codartis.NsDepCop.Setup.CustomActions
{
    /// <summary>
    /// This class implements custom install/uninstall actions.
    /// </summary>
    public static class NsDepCopCustomActions 
    {
        /// <summary>
        /// Modifies an MSBuild targets file to insert the NsDepCop custom action into the C# projects's build workflow.
        /// </summary>
        /// <param name="session">Windows installer context object.</param>
        /// <returns>
        /// ActionResult.Success if successful. 
        /// ActionResult.Failure if failed. 
        /// ActionResult.NotExecuted if the msbuild file could not be safely modified because 'BuildDependsOn' had custom content.
        /// In this case the msbuild file must be edited manually (add 'NsDepCop' to 'BuildDependsOn').
        /// </returns>
        /// <remarks>
        /// The full path of the MsBuild targets file to be modified must be given in CustomActionData["MSBUILDFILETOMODIFY"].
        /// </remarks>
        [CustomAction]
        public static ActionResult AddNsDepCopToMsBuildWorkflow(Session session)
        {
            var result = ActionResult.Success;

            try
            {
                var filename = session.CustomActionData["MSBUILDFILETOMODIFY"];
                if (string.IsNullOrWhiteSpace(filename))
                    return ActionResult.Failure;

                var xDocument = LoadOrCreateXDocument(filename, session);

                if (!AddBuildDependsOnPropertyGroup(xDocument, session))
                {
                    var record = new Record
                    {
                        FormatString = string.Format(
                            "Setup is unable to add NsDepCop target to the C# build workflow, " +
                            "because of custom content in {0}\n" +
                            "You have to add it manually. " +
                            "See the FAQ at http://nsdepcop.codeplex.com/documentation for more info. " +
                            "Setup continues.", filename)
                    };
                    session.Message(InstallMessage.Warning, record);
                    result = ActionResult.NotExecuted;
                }

                AddUsingTaskElement(xDocument, session);

                AddNsDepCopTarget(xDocument, session);

                session.Log("Saving XML file '{0}'", filename);
                xDocument.Save(filename);
            }
            catch (Exception e)
            {
                session.Log("Exception: {0}", e);
                return ActionResult.Failure;
            }

            return result;
        }

        /// <summary>
        /// Modifies an MSBuild targets file to remove the NsDepCop custom action from the C# projects's build workflow.
        /// </summary>
        /// <param name="session">Windows installer context object.</param>
        /// <returns>
        /// ActionResult.Success if successful. 
        /// ActionResult.Failure if failed. 
        /// ActionResult.NotExecuted if the msbuild file could not be safely modified because 'BuildDependsOn' had custom content.
        /// In this case the msbuild file must be edited manually (remove 'NsDepCop' from 'BuildDependsOn').
        /// </returns>
        /// <remarks>
        /// The full path of the MsBuild targets file to be modified must be given in CustomActionData["MSBUILDFILETOMODIFY"].
        /// </remarks>
        [CustomAction]
        public static ActionResult RemoveNsDepCopFromMsBuildWorkflow(Session session)
        {
            var result = ActionResult.Success;

            try
            {
                var filename = session.CustomActionData["MSBUILDFILETOMODIFY"];
                if (string.IsNullOrWhiteSpace(filename))
                    return ActionResult.Failure;

                if (!File.Exists(filename))
                    return result;

                session.Log("Loading XML file '{0}'", filename);
                var xDocument = XDocument.Load(filename);

                if (!RemoveBuildDependsOnPropertyGroup(xDocument, session))
                {
                    var record = new Record
                    {
                        FormatString = string.Format(
                            "Setup is unable to remove NsDepCop target from the C# build workflow, " +
                            "because of custom content in {0}\n" +
                            "You have to remove it manually. " +
                            "See the FAQ at http://nsdepcop.codeplex.com/documentation for more info. " +
                            "Setup continues.", filename)
                    };
                    session.Message(InstallMessage.Warning, record);
                    result = ActionResult.NotExecuted;
                }

                RemoveUsingTaskElement(xDocument, session);

                RemoveNsDepCopTarget(xDocument, session);

                session.Log("Saving XML file '{0}'", filename);
                xDocument.Save(filename);
            }
            catch (Exception e)
            {
                session.Log("Exception: {0}", e);
                return ActionResult.Failure;
            }

            return result;
        }

        /// <summary>
        /// Load a file into an XDocument. If the file does not exists then creates an empty msbuild project document.
        /// </summary>
        /// <param name="filename">The file to be loaded.</param>
        /// <param name="session">Windows installer context object (for logging).</param>
        /// <returns>An XDocument object.</returns>
        private static XDocument LoadOrCreateXDocument(string filename, Session session)
        {
            XDocument xDocument;

            if (File.Exists(filename))
            {
                session.Log("Loading XML file '{0}'", filename);

                xDocument = XDocument.Load(filename);
            }
            else
            {
                session.Log("Creating XML file '{0}'", filename);

                XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
                var projectElement = new XElement(ns + "Project");
                xDocument = new XDocument();
                xDocument.Add(projectElement);
            }

            return xDocument;
        }

        /// <summary>
        /// Adds the BuildDependsOn property group to an msbuild project document and sets NsDepCop as the last action.
        /// If the property group already exists and contains NsDepCop then does nothing.
        /// If the property group already exists without NsDepCop then the modification fails.
        /// </summary>
        /// <param name="xDocument">An msbuild project document.</param>
        /// <param name="session">Windows installer context object (for logging).</param>
        /// <returns>True if the modification was successful or unnecessary. False if the modification failed.</returns>
        private static bool AddBuildDependsOnPropertyGroup(XDocument xDocument, Session session)
        {
            var root = xDocument.Root;
            var ns = root.Name.Namespace;
            
            var existingBuildDependsOnElement = root.Elements(ns + "PropertyGroup").Elements(ns + "BuildDependsOn").FirstOrDefault();
            if (existingBuildDependsOnElement == null)
            {
                session.Log("PropertyGroup/BuildDependOn elements do not exist; creating.");

                var propertyGroupElement = new XElement(ns + "PropertyGroup");
                xDocument.Root.AddFirst(propertyGroupElement);
                var buildDependsOnElement = new XElement(ns + "BuildDependsOn", "$(BuildDependsOn);NsDepCop");
                propertyGroupElement.Add(buildDependsOnElement);

                return true;
            }

            if (existingBuildDependsOnElement.Value != null &&
                existingBuildDependsOnElement.Value.Contains("NsDepCop"))
            {
                session.Log("PropertyGroup/BuildDependOn already contains NsDepCop target.");
                return true;
            }

            session.Log("PropertyGroup/BuildDependOn already exists with custom content. Must be modified manually.");
            return false;
        }

        /// <summary>
        /// Removes the BuildDependsOn property group from an msbuild project document if it contains no other content than what setup created.
        /// If the property group is missing then does nothing.
        /// If the property group exists with other than the setup-created content then the modification fails.
        /// </summary>
        /// <param name="xDocument">An msbuild project document.</param>
        /// <param name="session">Windows installer context object (for logging).</param>
        /// <returns>True if the modification was successful or unnecessary. False if the modification failed.</returns>
        private static bool RemoveBuildDependsOnPropertyGroup(XDocument xDocument, Session session)
        {
            var root = xDocument.Root;
            var ns = root.Name.Namespace;

            var existingBuildDependsOnElement = root.Elements(ns + "PropertyGroup").Elements(ns + "BuildDependsOn").FirstOrDefault();
            if (existingBuildDependsOnElement == null)
            {
                session.Log("PropertyGroup/BuildDependOn elements not found. Nothing to do.");
                return true;
            }

            if (existingBuildDependsOnElement.Value == null ||
                !existingBuildDependsOnElement.Value.Contains("NsDepCop"))
            {
                session.Log("PropertyGroup/BuildDependOn does not contain NsDepCop. Leaving it as is.");
                return true;
            }

            if (existingBuildDependsOnElement.Value == "$(BuildDependsOn);NsDepCop")
            {
                session.Log("Removing PropertyGroup/BuildDependOn elements.");
                existingBuildDependsOnElement.Parent.Remove();
                return true;
            }

            session.Log("PropertyGroup/BuildDependOn exists with customized content. Must be removed manually.");
            return false;
        }

        /// <summary>
        /// Adds a UsingTask element defining the NsDepCop task to the msbuild project document.
        /// </summary>
        /// <param name="xDocument">An msbuild project document.</param>
        /// <param name="session">Windows installer context object (for logging).</param>
        private static void AddUsingTaskElement(XDocument xDocument, Session session)
        {
            var root = xDocument.Root;
            var ns = root.Name.Namespace;

            if (root.Elements(ns + "UsingTask").Attributes("TaskName").Any(a => a.Value == "Codartis.NsDepCop.MsBuildTask.NsDepCopTask"))
            {
                session.Log("UsingTask element already exists for NsDepCopTask.");
            }
            else
            {
                session.Log("UsingTask element does not exist for NsDepCopTask; creating.");

                var usingTaskElement = new XElement(ns + "UsingTask");
                xDocument.Root.AddFirst(usingTaskElement);
                usingTaskElement.Add(new XAttribute("AssemblyFile", @"NsDepCop.MsBuildTask.dll"));
                usingTaskElement.Add(new XAttribute("TaskName", "Codartis.NsDepCop.MsBuildTask.NsDepCopTask"));
            }
        }

        /// <summary>
        /// Removes the UsingTask element defining the NsDepCop task from the msbuild project.
        /// </summary>
        /// <param name="xDocument">An msbuild project document.</param>
        /// <param name="session">Windows installer context object (for logging).</param>
        private static void RemoveUsingTaskElement(XDocument xDocument, Session session)
        {
            var root = xDocument.Root;
            var ns = root.Name.Namespace;

            var usingTaskElementTaskNameAttribute = root.Elements(ns + "UsingTask")
                .Attributes("TaskName").FirstOrDefault(a => a.Value == "Codartis.NsDepCop.MsBuildTask.NsDepCopTask");
            if (usingTaskElementTaskNameAttribute != null)
            {
                session.Log("Removing UsingTask element.");
                usingTaskElementTaskNameAttribute.Parent.Remove();
            }
            else
            {
                session.Log("UsingTask element not found.");
            }
        }

        /// <summary>
        /// Adds the NsDepCop target element to the msbuild project document.
        /// </summary>
        /// <param name="xDocument">An msbuild project document.</param>
        /// <param name="session">Windows installer context object (for logging).</param>
        private static void AddNsDepCopTarget(XDocument xDocument, Session session)
        {
            var root = xDocument.Root;
            var ns = root.Name.Namespace;

            if (root.Elements(ns + "Target").Attributes("Name").Any(a => a.Value == "NsDepCop"))
            {
                session.Log("NsDepCop target already exists.");
            }
            else
            {
                session.Log("NsDepCop target does not exist; creating.");

                var targetElement = new XElement(ns + "Target");
                xDocument.Root.Add(targetElement);
                targetElement.Add(new XAttribute("Name", "NsDepCop"));

                var nsDepCopTaskElement = new XElement(ns + "NsDepCopTask");
                targetElement.Add(nsDepCopTaskElement);
                nsDepCopTaskElement.Add(new XAttribute("ReferencePath", "@(ReferencePath)"));
                nsDepCopTaskElement.Add(new XAttribute("Compile", "@(Compile)"));
                nsDepCopTaskElement.Add(new XAttribute("BaseDirectory", "$(MSBuildProjectDirectory)"));
            }
        }

        /// <summary>
        /// Removes the NsDepCop target element from the msbuild project document.
        /// </summary>
        /// <param name="xDocument">An msbuild project document.</param>
        /// <param name="session">Windows installer context object (for logging).</param>
        private static void RemoveNsDepCopTarget(XDocument xDocument, Session session)
        {
            var root = xDocument.Root;
            var ns = root.Name.Namespace;

            var targetElementNameAttribute = root.Elements(ns + "Target").Attributes("Name").FirstOrDefault(a => a.Value == "NsDepCop");
            if (targetElementNameAttribute != null)
            {
                session.Log("Removing NsDepCop target element.");
                targetElementNameAttribute.Parent.Remove();
            }
            else
            {
                session.Log("NsDepCop target element not found.");
            }
        }
    }
}
