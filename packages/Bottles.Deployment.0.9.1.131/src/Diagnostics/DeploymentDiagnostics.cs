using System;
using System.IO;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Diagnostics
{
    public class DeploymentDiagnostics : LoggingSession, IDeploymentDiagnostics
    {
        public void LogDeployment(DeploymentPlan plan)
        {
            //root log object
            LogObject(plan, "Deployment Profile: "+ plan.ProfileName);
        }

        public void LogHost(DeploymentPlan plan, HostManifest hostManifest)
        {
            var provenance = "Profile {0} / Host {1}".ToFormat(plan.ProfileName, hostManifest.Name);
            LogObject(hostManifest, provenance);
            LogFor(plan).AddChild(hostManifest);
        }

        public void LogDirective(HostManifest host, IDirective directive)
        {
            var provenance = "Found in '{0}'".ToFormat(host);
            LogObject(directive, provenance);
            LogFor(host).AddChild(directive);
        }

        public IPackageLog LogAction(HostManifest host, IDirective directive, object action, string description)
        {
            var provenance = "Host {0} / Directive {1}".ToFormat(host.Name, directive.GetType().Name);
            LogObject(action, provenance);
            LogFor(directive).AddChild(action);

            DeploymentController.RunningStep("{0} for {1}", description, provenance);

            return LogFor(action);
        }

        public LoggingSession Session
        {
            get { return this; }
        }

        public void AssertNoFailures()
        {
            if (!HasErrors()) return;


            var writer = new StringWriter();
            writer.WriteLine("Package loading and application bootstrapping failed");
            writer.WriteLine();
            EachLog((o, log) =>
            {
                if (!log.Success)
                {
                    writer.WriteLine(o.ToString());
                    writer.WriteLine(log.FullTraceText());
                    writer.WriteLine(new string('-', 80));
                }
            });

            throw new ApplicationException(writer.GetStringBuilder().ToString());

        }
    }
}