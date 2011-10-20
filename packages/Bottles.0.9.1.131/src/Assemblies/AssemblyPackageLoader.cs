using System.Collections.Generic;
using System.Reflection;
using Bottles.Diagnostics;

namespace Bottles.Assemblies
{
    public class AssemblyPackageLoader : IPackageLoader
    {
        private readonly Assembly _assembly;

        public AssemblyPackageLoader(Assembly assembly)
        {
            _assembly = assembly;
        }

        public IEnumerable<IPackageInfo> Load(IPackageLog log)
        {
            yield return AssemblyPackageInfo.CreateFor(_assembly);
        }

        public override string ToString()
        {
            return string.Format("Assembly: {0}", _assembly);
        }

        public bool Equals(AssemblyPackageLoader other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._assembly, _assembly);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (AssemblyPackageLoader)) return false;
            return Equals((AssemblyPackageLoader) obj);
        }

        public override int GetHashCode()
        {
            return (_assembly != null ? _assembly.GetHashCode() : 0);
        }
    }
}