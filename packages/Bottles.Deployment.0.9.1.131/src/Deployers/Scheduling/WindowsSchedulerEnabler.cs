using System;
using System.Diagnostics;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.Scheduling
{
    public class WindowsSchedulerEnabler : IFinalizer<ScheduledTask>
    {
        private readonly IProcessRunner _runner;

        public WindowsSchedulerEnabler(IProcessRunner runner)
        {
            _runner = runner;
        }

        public void Execute(ScheduledTask directive, HostManifest host, IPackageLog log)
        {
            log.Trace("Enabling the scheduled task {0}".ToFormat(directive.Name));
            var psi = new ProcessStartInfo("schtasks");
            var args = "/change /tn {0} /ENABLE".ToFormat(directive.Name);
            psi.Arguments = args;
            log.Trace(args);
            _runner.Run(psi, new TimeSpan(0,0,1,0)).AssertMandatorySuccess();
        }

        public string GetDescription(ScheduledTask directive)
        {
            return "Enabling the scheduled task '{0}' if it exists".ToFormat(directive.Name);
        }
    }
}