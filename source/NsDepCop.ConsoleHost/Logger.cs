using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using Console = System.Console;

namespace Codartis.NsDepCop.ConsoleHost
{
    public class Logger : ILogger
    {
        public void Initialize(IEventSource eventSource)
        {
            eventSource.ErrorRaised += EventSource_AnyEventRaised;
        }

        private void EventSource_AnyEventRaised(object sender, BuildEventArgs e)
        {
           Console.WriteLine(e.Message);
        }

        public void Shutdown()
        {
        }

        public LoggerVerbosity Verbosity { get; set; }
        public string Parameters { get; set; }
    }
}
