using System;
using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public class EnvironmentProxy : MarshalByRefObject
    {
        public override object InitializeLifetimeService()
        {
            return null;
        }

        public IEnumerable<LogEntry> Install(EnvironmentRun run)
        {
            return execute(run, (installer, log) => installer.Install(log));
        }

        public IEnumerable<LogEntry> CheckEnvironment(EnvironmentRun run)
        {
            return execute(run, (installer, log) => installer.CheckEnvironment(log));
        }

        public IEnumerable<LogEntry> InstallAndCheckEnvironment(EnvironmentRun run)
        {
            return execute(run, (installer, log) => installer.Install(log),
                                (installer, log) => installer.CheckEnvironment(log));
        }

        private IEnumerable<LogEntry> execute(EnvironmentRun run, params Action<IInstaller, IPackageLog>[] actions)
        {
            var runner = new EnvironmentRunner(run);
            return runner.ExecuteEnvironment(actions);
        }
    }
}