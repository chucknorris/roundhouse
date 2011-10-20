using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Bottles.Assemblies;
using System.Linq;

namespace Bottles
{
    [DebuggerDisplay("{Name}:{Role}")]
    public class PackageInfo : IPackageInfo
    {
        
        private readonly PackageFiles _files = new PackageFiles();
        private readonly IList<AssemblyTarget> _assemblies = new List<AssemblyTarget>();
        private readonly IList<Dependency> _dependencies = new List<Dependency>();

        public PackageInfo(string name)
        {
            Name = name;
        }

        public void RegisterFolder(string folderName, string directory)
        {
            _files.RegisterFolder(folderName, directory);
        }

        public string Description
        {
            get; set;
        }

        public void RegisterAssemblyLocation(string assemblyName, string filePath)
        {
            _assemblies.Add(new AssemblyTarget(){
                AssemblyName = assemblyName, 
                FilePath = filePath
            });
        }

        public void AddDependency(Dependency dependency)
        {
            _dependencies.Fill(dependency);
        }

        public class AssemblyTarget
        {
            public string AssemblyName { get; set;}
            public string FilePath { get; set;}

            public void Load(IAssemblyRegistration loader)
            {
                loader.LoadFromFile(FilePath, AssemblyName);
            }
        }

        public string Name { get; private set; }

        public string Role { get; set; }

        public Dependency[] Dependencies
        {
            get { return _dependencies.ToArray(); }
            set
            {
                _dependencies.Clear();
                _dependencies.AddRange(value);
            }
        }

        void IPackageInfo.LoadAssemblies(IAssemblyRegistration loader)
        {
            _assemblies.Each(a => a.Load(loader));
        }

        void IPackageInfo.ForFolder(string folderName, Action<string> onFound)
        {
            _files.ForFolder(folderName, onFound);
        }

        void IPackageInfo.ForData(string searchPattern, Action<string, Stream> dataCallback)
        {
            _files.ForData(searchPattern, dataCallback);
        }

        IEnumerable<Dependency> IPackageInfo.GetDependencies()
        {
            return _dependencies;
        }

        public override string ToString()
        {
            return Description;
        }

        public bool Equals(PackageInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Name, Name) && Equals(other.Description, Description);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PackageInfo)) return false;
            return Equals((PackageInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0)*397) ^ (Description != null ? Description.GetHashCode() : 0);
            }
        }
    }
}