using System;
using System.Collections.Generic;
using System.Linq;
using Bottles.Deployment.Diagnostics;
using Bottles.Diagnostics;
using Bottles.Environment;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class InstallationRunner
    {
        private readonly IEnvironmentGateway _gateway;
        private readonly IInstallationLogger _logger;

        public InstallationRunner(IEnvironmentGateway gateway, IInstallationLogger logger)
        {
            _gateway = gateway;
            _logger = logger;
        }

        // TODO -- this needs to fail!
        public void RunTheInstallation(InstallInput input)
        {
            var entries = execute(input);
            logEntries(input, entries);
        }

        private void logEntries(InstallInput input, IEnumerable<LogEntry> entries)
        {
            _logger.WriteLogsToConsole(entries);
            _logger.WriteLogsToFile(input, entries);

            if (entries.Any(x => !x.Success))
            {
                _logger.WriteFailureToConsole();
            }
            else
            {
                _logger.WriteSuccessToConsole();
            }
        }

        private IEnumerable<LogEntry> execute(InstallInput input)
        {
            ConsoleWriter.Write(input.Title());

            switch (input.ModeFlag)
            {
                case InstallMode.install:
                    return _gateway.Install();

                case InstallMode.check:
                    return _gateway.CheckEnvironment();

                case InstallMode.all:
                    return _gateway.InstallAndCheckEnvironment();
            }

            return new LogEntry[0];
        }
    }
}