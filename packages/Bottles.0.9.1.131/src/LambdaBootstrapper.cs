using System;
using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles
{
    public class LambdaBootstrapper : IBootstrapper
    {
        private readonly Func<IPackageLog, IEnumerable<IActivator>> _bootstrapper;

        public LambdaBootstrapper(Func<IPackageLog, IEnumerable<IActivator>> bootstrapper)
        {
            _bootstrapper = bootstrapper;
        }

        public string Provenance { get; set; }

        public IEnumerable<IActivator> Bootstrap(IPackageLog log)
        {
            return _bootstrapper(log);
        }

        public override string ToString()
        {
            return string.Format("Lambda expression at: {0}", Provenance);
        }
    }
}