using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bottles.Diagnostics
{
    public interface IPackagingDiagnostics
    {
        void LogObject(object target, string provenance);
        void LogPackage(IPackageInfo package, IPackageLoader loader);
        void LogBootstrapperRun(IBootstrapper bootstrapper, IEnumerable<IActivator> activators);
        void LogAssembly(IPackageInfo package, Assembly assembly, string provenance);
        void LogDuplicateAssembly(IPackageInfo package, string assemblyName);
        void LogAssemblyFailure(IPackageInfo package, string fileName, Exception exception);
        void LogExecution(object target, Action continuation);
        IPackageLog LogFor(object target);
    }
}