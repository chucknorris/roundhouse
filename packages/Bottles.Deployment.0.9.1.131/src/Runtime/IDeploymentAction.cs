using System;
using Bottles.Diagnostics;

namespace Bottles.Deployment.Runtime
{
    public interface IDeploymentAction<T> where T : IDirective
    {
        void Execute(T directive, HostManifest host, IPackageLog log);
        string GetDescription(T directive);
    }
}