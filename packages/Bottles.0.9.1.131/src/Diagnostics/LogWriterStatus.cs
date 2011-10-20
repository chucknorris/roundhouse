using System.Collections.Generic;

namespace Bottles.Diagnostics
{
    public class LogWriterStatus
    {
        private readonly Stack<IPackageLog> _logs = new Stack<IPackageLog>();

        public LogWriterStatus()
        {
            _logs.Push(new PackageLog());
        }

        public IPackageLog Current
        {
            get
            {
                return _logs.Peek();
            }
        }

        public void PushLog(IPackageLog log)
        {
            _logs.Push(log);
        }

        public void PopLog()
        {
            _logs.Pop();
        }
    }
}