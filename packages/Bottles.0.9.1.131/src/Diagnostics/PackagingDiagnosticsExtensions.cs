using System;
using System.Collections.Generic;

namespace Bottles.Diagnostics
{
    public static class PackagingDiagnosticsExtensions
    {
        public static void LogExecutionOnEach<T>(this IPackagingDiagnostics diagnostics, IEnumerable<T> targets, Action<T> continuation)
        {
            targets.Each(t =>
            {
                diagnostics.LogExecution(t, () => continuation(t));
            });
        }

        public static void LogPackages(this IPackagingDiagnostics diagnostics, IPackageLoader loader, IEnumerable<IPackageInfo> packages)
        {
            packages.Each(p =>
            {
                diagnostics.LogPackage(p, loader);
            });
        }
    }
}