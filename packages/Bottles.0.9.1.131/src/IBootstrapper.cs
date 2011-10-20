using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles
{
    public interface IBootstrapper
    {
        IEnumerable<IActivator> Bootstrap(IPackageLog log);
    }
}