using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using FubuCore;

namespace Bottles
{
    [XmlType("package")]
    public class PackageManifest
    {
        public static readonly string FILE = ".package-manifest";

        public static FileSet FileSetForSearching()
        {
            return new FileSet(){
                DeepSearch = true,
                Include = FILE
            };
        }

        public static IEnumerable<string> FindManifestFilesInDirectory(string directory)
        {
            return new FileSystem().FindFiles(directory, FileSetForSearching());
        }

        public PackageManifest()
        {
            Role = BottleRoles.Module;
        }

        [XmlIgnore]
        public string ManifestFileName { get; set; }

        private readonly IList<string> _assemblies = new List<string>();

        


        public string Role { get; set; }
        public string Name { get; set; }
        public string BinPath { get; set; }

        [XmlElement("assembly")]
        public string[] Assemblies 
        {
            get
            {
                return _assemblies.ToArray();
            }
            set
            {
                _assemblies.Clear();

                if (value == null) return;
                _assemblies.AddRange(value);
            }
        }

        public bool AddAssembly(string assemblyName)
        {
            if (_assemblies.Contains(assemblyName))
            {
                return false;
            }

            _assemblies.Add(assemblyName);
            return true;
        }

        public FileSet DataFileSet
        {
            get; set;
        }

        public FileSet ContentFileSet
        {
            get; set;
        }

        public FileSet ConfigFileSet
        {
            get; set;
        }



        public override string ToString()
        {
            return string.Format("Package: {0}", Name);
        }

        public void RemoveAllAssemblies()
        {
            _assemblies.Clear();
        }

        public void RemoveAssembly(string assemblyName)
        {
            _assemblies.Remove(assemblyName);
        }

        public void SetRole(string role)
        {
            Role = role;



            switch (role)
            {
                case BottleRoles.Config:
                    ConfigFileSet = new FileSet(){Include = "*.*", DeepSearch = true};
                    ContentFileSet = null;
                    DataFileSet = null;
                    RemoveAllAssemblies();
                    break;

                case BottleRoles.Binaries:
                    ConfigFileSet = null;
                    ContentFileSet = null;
                    DataFileSet = null;
                    break;

                case BottleRoles.Data:
                    ConfigFileSet = null;
                    ContentFileSet = null;
                    RemoveAllAssemblies();
                    DataFileSet = new FileSet(){DeepSearch = true, Include = "*.*"};
                    break;

                default:
                    DataFileSet = new FileSet();
                    ContentFileSet = new FileSet()
                    {
                        Include = "*.as*x;*.master;Content{0}*.*;*.config".ToFormat(Path.DirectorySeparatorChar),
                        Exclude = "data/*"
                    };
                    break;

            }
        }

        private readonly IList<Dependency> _dependencies = new List<Dependency>();

        [XmlElement("dependency")]
        public Dependency[] Dependencies
        {
            get
            {
                return _dependencies.ToArray();
            }
            set
            {
                _dependencies.Clear();
                if (value != null) _dependencies.AddRange(value);
            }
        }

        public void AddDependency(string packageName, bool isMandatory)
        {
            _dependencies.RemoveAll(x => x.Name == packageName);
            _dependencies.Add(new Dependency(){
                IsMandatory = isMandatory,
                Name = packageName
            });
        }

        public void WriteTo(string directory)
        {
            new FileSystem().WriteObjectToFile(directory.AppendPath(FILE), this);
        }
    }
}