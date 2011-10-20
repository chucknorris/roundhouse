using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles
{
    public interface IActivator
    {
        void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log);
    }
}