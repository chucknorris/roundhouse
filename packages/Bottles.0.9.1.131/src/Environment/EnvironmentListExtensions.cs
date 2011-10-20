using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    public static class EnvironmentListExtensions
    {
        public static void Add(this IList<LogEntry> list, object target, PackageLog log)
        {
            list.Add(LogEntry.FromPackageLog(target, log));
        }
    }
}