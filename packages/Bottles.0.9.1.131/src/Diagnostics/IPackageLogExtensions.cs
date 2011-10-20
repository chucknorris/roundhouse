using System;

namespace Bottles.Diagnostics
{
    public static class IPackageLogExtensions
    {
        public static void TrapErrors(this IPackageLog log, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                log.MarkFailure(e);
            }
        }
    }
}