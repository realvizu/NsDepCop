using System;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// A gateway for acquiring Visual Studio services.
    /// </summary>
    public static class VisualStudioServiceGateway
    {
        public static IVsActivityLog GetActivityLogService()
        {
            var serviceProvider = GetGlobalServiceProvider();

            var activityLog = (IVsActivityLog)serviceProvider.GetService(typeof(SVsActivityLog));
            if (activityLog == null)
                throw new Exception("Cannot acquire SVsActivityLog.");
            return activityLog;
        }

        public static Workspace GetWorkspace()
        {
            var componentModel = GetComponentModelService();

            var visualStudioWorkspace = componentModel.GetService<VisualStudioWorkspace>();
            if (visualStudioWorkspace == null)
                throw new Exception("Cannot acquire VisualStudioWorkspace.");
            return visualStudioWorkspace;
        }

        private static IComponentModel GetComponentModelService()
        {
            var serviceProvider = GetGlobalServiceProvider();

            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            if (componentModel == null)
                throw new Exception("Cannot acquire SComponentModel.");
            return componentModel;
        }

        private static IServiceProvider GetGlobalServiceProvider()
        {
            var serviceProvider = ServiceProvider.GlobalProvider;
            if (serviceProvider == null)
                throw new Exception("Cannot acquire ServiceProvider.GlobalProvider.");
            return serviceProvider;
        }
    }
}