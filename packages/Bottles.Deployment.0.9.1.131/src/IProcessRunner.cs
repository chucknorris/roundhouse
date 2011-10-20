using System;
using System.Diagnostics;
using Bottles.Diagnostics;

namespace Bottles.Deployment
{
    public interface IProcessRunner
    {
        ProcessReturn Run(ProcessStartInfo info, TimeSpan waitDuration);
        ProcessReturn Run(ProcessStartInfo info);
    }
}