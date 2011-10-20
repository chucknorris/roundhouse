using System;
using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public class EnvironmentGateway : IEnvironmentGateway
    {
        private readonly EnvironmentRun _run;

        public EnvironmentGateway(EnvironmentRun run)
        {
            _run = run;
        }

        public IEnumerable<LogEntry> Install()
        {
            return RunEnvironment(p => p.Install(_run));
        }

        public IEnumerable<LogEntry> CheckEnvironment()
        {
            return RunEnvironment(p => p.CheckEnvironment(_run));
        }

        public IEnumerable<LogEntry> InstallAndCheckEnvironment()
        {
            return RunEnvironment(p => p.InstallAndCheckEnvironment(_run));
        }

        public IEnumerable<LogEntry> RunEnvironment(Func<EnvironmentProxy, IEnumerable<LogEntry>> func)
        {
            AppDomain domain = null;

            try
            {
                var setup = _run.BuildAppDomainSetup();
                domain = AppDomain.CreateDomain("Bottles Environment Setup", null, setup);
                var proxy =
                    (EnvironmentProxy) domain.CreateInstanceAndUnwrap(typeof (EnvironmentProxy).Assembly.FullName,
                                                                      typeof (EnvironmentProxy).FullName);

                return func(proxy);
            }
            finally
            {
                if (domain != null)
                {
                    AppDomain.Unload(domain);
                    domain = null;
                }
            }
        }
    }
}