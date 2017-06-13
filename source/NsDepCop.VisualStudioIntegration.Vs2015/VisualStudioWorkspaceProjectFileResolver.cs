using System;
using System.Linq;
using Codartis.NsDepCop.Core.Util;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;

namespace Codartis.NsDepCop.VisualStudioIntegration
{
    /// <summary>
    /// Finds project files in the current Visual Studio workspace.
    /// </summary>
    public class VisualStudioWorkspaceProjectFileResolver : IProjectFileResolver
    {
        private readonly IComponentModel _componentModel;
        private readonly MessageHandler _infoMessageHandler;
        private readonly MessageHandler _diagnosticMessageHandler;

        public VisualStudioWorkspaceProjectFileResolver(IComponentModel componentModel,
            MessageHandler infoMessageHandler = null,
            MessageHandler diagnosticMessageHandler = null)
        {
            if (componentModel == null)
                throw new ArgumentNullException(nameof(componentModel));

            _componentModel = componentModel;
            _infoMessageHandler = infoMessageHandler;
            _diagnosticMessageHandler = diagnosticMessageHandler;
        }

        public string FindByAssemblyName(string assemblyName)
        {
            var visualStudioWorkspace = _componentModel.GetService<VisualStudioWorkspace>();
            if (visualStudioWorkspace == null)
            {
                _infoMessageHandler?.Invoke("Cannot acquire VisualStudioWorkspace.");
                return null;
            }

            var currentSolution = visualStudioWorkspace.CurrentSolution;
            if (currentSolution == null)
            {
                _infoMessageHandler?.Invoke("Cannot acquire CurrentSolution.");
                return null;
            }

            return currentSolution.Projects.FirstOrDefault(i => i.AssemblyName == assemblyName)?.FilePath;
        }
    }
}
