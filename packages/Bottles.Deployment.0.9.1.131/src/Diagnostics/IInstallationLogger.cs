using System.Collections.Generic;
using Bottles.Deployment.Commands;
using Bottles.Diagnostics;

namespace Bottles.Deployment.Diagnostics
{
    public interface IInstallationLogger
    {
        void WriteLogsToConsole(IEnumerable<LogEntry> entries);
        void WriteLogsToFile(InstallInput input, IEnumerable<LogEntry> entries);
        void WriteSuccessToConsole();
        void WriteFailureToConsole();
    }
}