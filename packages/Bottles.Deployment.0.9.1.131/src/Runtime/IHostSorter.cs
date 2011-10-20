using System;
using System.Collections.Generic;
using FubuCore.DependencyAnalysis;

namespace Bottles.Deployment.Runtime
{
    public interface IHostSorter
    {
        IEnumerable<HostManifest> Sort(IEnumerable<HostManifest> hosts);
    }

    public class HostSorter :IHostSorter
    {
        public IEnumerable<HostManifest> Sort(IEnumerable<HostManifest> hosts)
        {
            //REVIEW: no host dependencies listed
            //var graph = new DependencyGraph<HostManifest>(h => h.Name, h => "");
            return hosts;
        }
    }
}