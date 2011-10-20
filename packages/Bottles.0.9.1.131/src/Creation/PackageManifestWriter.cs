using System;
using FubuCore;

namespace Bottles.Creation
{
    public class PackageManifestWriter
    {
        private readonly IFileSystem _fileSystem;
        private PackageManifest _manifest;

        public PackageManifestWriter(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        //REVIEW: why is this reading?
        public void ReadFrom(string fileName, Action<PackageManifest> onCreation)
        {
            if (_fileSystem.FileExists(fileName))
            {
                _manifest = _fileSystem.LoadFromFile<PackageManifest>(fileName);
            }
            else
            {
                _manifest = new PackageManifest();
                onCreation(_manifest);
            }
        }

        public PackageManifest Manifest
        {
            get { return _manifest; }
        }

        public void WriteTo(string fileName)
        {
            _fileSystem.WriteObjectToFile(fileName, _manifest);
        }

        public void AddAssembly(string assemblyName)
        {
            Manifest.AddAssembly(assemblyName);
        }
    }
}