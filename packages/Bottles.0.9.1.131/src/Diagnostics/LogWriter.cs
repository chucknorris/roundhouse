using System;

namespace Bottles.Diagnostics
{
    public static class LogWriter
    {
        private static readonly LogWriterStatus _status = new LogWriterStatus();

        public static void WithLog(IPackageLog log, Action action)
        {
            _status.PushLog(log);
            try
            {
                action();
            }
            finally
            {
                _status.PopLog();
            }
        }

        public static IPackageLog Current
        {
            get { return _status.Current; }
        }
    }
}