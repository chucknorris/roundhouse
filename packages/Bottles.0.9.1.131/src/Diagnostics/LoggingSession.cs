using System;
using System.Linq;
using FubuCore.Util;

namespace Bottles.Diagnostics
{
    public class LoggingSession
    {
        private readonly Cache<object, PackageLog> _logs = new Cache<object, PackageLog>(o => new PackageLog{
            Description = o.ToString()
        });

        public void LogObject(object target, string provenance)
        {
            _logs[target].Provenance = provenance;
        }

        public IPackageLog LogFor(object target)
        {
            return _logs[target];
        }

        public void LogExecution(object target, Action continuation)
        {
            _logs[target].Execute(continuation);
        }

        public void EachLog(Action<object, PackageLog> action)
        {
            _logs.Each(action);
        }

        public bool HasErrors()
        {
            return _logs.GetAll().Any(x => !x.Success);
        }
    }
}