using System;
using System.Collections.Generic;
using Bottles.Deployment.Commands;
using Bottles.Diagnostics;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Diagnostics
{
    public class InstallationLogger : IInstallationLogger
    {
        public void WriteLogsToConsole(IEnumerable<LogEntry> entries)
        {
            entries.Each(writeLog);
        }

        public void WriteLogsToFile(InstallInput input, IEnumerable<LogEntry> entries)
        {
            var document = EntryLogWriter.Write(entries, input.Title() + " at " + DateTime.UtcNow.ToLongDateString());
            document.WriteToFile(input.LogFileFlag);

            ConsoleWriter.Write("Output writing to {0}", input.LogFileFlag.ToFullPath());

            if (input.OpenFlag)
            {
                document.OpenInBrowser();
            }
        }

        public void WriteSuccessToConsole()
        {
            ConsoleWriter.Write("All installers succeeded without problems");
        }

        public void WriteFailureToConsole()
        {
            throw new CommandFailureException("Failures occurred.  Please see the log file");
        }

        private void writeLog(LogEntry log)
        {
            ConsoleWriter.Write("{0}, Success = {1}", log.Description, log.Success);
            ConsoleWriter.Write(log.TraceText);

            ConsoleWriter.Line();
            ConsoleWriter.Line();
        }
    }
}