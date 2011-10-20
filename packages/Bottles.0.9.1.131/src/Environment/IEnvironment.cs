using System;
using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles.Environment
{
    /// <summary>
    /// The implementation of this class is used in the 'Environment Run'
    /// </summary>
    public interface IEnvironment : IDisposable
    {

        IEnumerable<IInstaller> StartUp(IPackageLog log);
    }
}