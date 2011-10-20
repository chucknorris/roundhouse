using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles
{
    public interface IPackageLoader
    {
        IEnumerable<IPackageInfo> Load(IPackageLog log);
    }
}