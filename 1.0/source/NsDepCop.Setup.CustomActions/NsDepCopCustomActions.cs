using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Deployment.WindowsInstaller;

namespace Codartis.NsDepCop.Setup.CustomActions
{
    public static class NsDepCopCustomActions
    {
        [CustomAction]
        public static ActionResult AddNsDepCopTaskToTargetsFile(Session session)
        {
            ActionResult result = ActionResult.Success;

            try
            {
                var filename = @"C:\Program Files (x86)\MSBuild\v4.0\Custom.After.Microsoft.CSharp.targets";

                var xDocument = LoadOrCreateXDocument(filename, session);

                if (!AddBuildDependsOnPropertyGroup(xDocument, session))
                    result = ActionResult.NotExecuted;

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

        [CustomAction]
        public static ActionResult RemoveNsDepCopTaskFromTargetsFile(Session session)
        {
            ActionResult result = ActionResult.Success;

            try
            {
                var filename = @"C:\Program Files (x86)\MSBuild\v4.0\Custom.After.Microsoft.CSharp.targets";

                if (!File.Exists(filename))
                    return result;

                session.Log("Loading XML file '{0}'", filename);
                var xDocument = XDocument.Load(filename);

                if (!RemoveBuildDependsOnPropertyGroup(xDocument, session))
                    result = ActionResult.NotExecuted;

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

        private static XDocument LoadOrCreateXDocument(string filename, Session session)
        {
            XDocument xDocument = null;

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
            }
            else
            {
                session.Log("PropertyGroup/BuildDependOn elements already exist.");

                if (existingBuildDependsOnElement.Value.Contains("NsDepCop"))
                {
                    session.Log("PropertyGroup/BuildDependOn already contains NsDepCop target.");
                }
                else
                {
                    session.Log("PropertyGroup/BuildDependOn already exists with unknown content; must be modified manually.");
                    return false;
                }
            }

            return true;
        }

        private static bool RemoveBuildDependsOnPropertyGroup(XDocument xDocument, Session session)
        {
            var root = xDocument.Root;
            var ns = root.Name.Namespace;

            var existingBuildDependsOnElement = root.Elements(ns + "PropertyGroup").Elements(ns + "BuildDependsOn").FirstOrDefault();
            if (existingBuildDependsOnElement == null)
            {
                session.Log("PropertyGroup/BuildDependOn elements not found.");
            }
            else
            {
                if (existingBuildDependsOnElement.Value == "$(BuildDependsOn);NsDepCop")
                {
                    session.Log("Removing PropertyGroup/BuildDependOn elements.");
                    existingBuildDependsOnElement.Parent.Remove();
                }
                else
                {
                    session.Log("PropertyGroup/BuildDependOn exists with customized content; must be removed manually.");
                    return false;
                }
            }

            return true;
        }

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
