using System;
using System.Diagnostics;

namespace FubuCore
{
    public static class NumberExtensions
    {
        [DebuggerStepThrough]
        public static int Times(this int maxCount, Action<int> eachAction)
        {
            for (int idx = 0; idx < maxCount; idx++)
            {
                eachAction(idx);
            }

            return maxCount;
        }
    }
}