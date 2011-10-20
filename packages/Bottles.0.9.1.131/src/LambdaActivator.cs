using System;
using System.Collections.Generic;
using Bottles.Diagnostics;

namespace Bottles
{
    public class LambdaActivator : IActivator
    {
        private readonly string _description;
        private readonly Action _action;

        public LambdaActivator(string description, Action action)
        {
            _description = description;
            _action = action;
        }

        public void Activate(IEnumerable<IPackageInfo> packages, IPackageLog log)
        {
            _action();
        }

        public override string ToString()
        {
            return _description;
        }
    }

    public class LambdaActivator<T> : LambdaActivator
    {
        public LambdaActivator(T service, string description, Action<T> action) : base(description, () => action(service))
        {
        }
    }
}