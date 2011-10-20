using System;

namespace Bottles.Diagnostics
{
    [Serializable]
    public class LogEntry
    {
        public bool Success { get; set; }
        public string TraceText { get; set; }
        public string Description { get; set; }
        public long TimeInMilliseconds { get; set; }

        public static LogEntry FromPackageLog(object target, IPackageLog log)
        {
            return new LogEntry(){
                Description = target.ToString(),
                Success = log.Success,
                TraceText = log.FullTraceText().Trim(),
                TimeInMilliseconds = log.TimeInMilliseconds
            };
        }

    }
}