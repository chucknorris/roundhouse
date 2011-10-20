using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public interface IEnvironmentGateway
    {
        IEnumerable<LogEntry> Install();
        IEnumerable<LogEntry> CheckEnvironment();
        IEnumerable<LogEntry> InstallAndCheckEnvironment();
    }
}