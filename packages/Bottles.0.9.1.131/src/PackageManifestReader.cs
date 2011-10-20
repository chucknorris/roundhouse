using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;


namespace Bottles
{
    public class PackageManifestReader : IPackageManifestReader
    {
        private readonly IFileSystem _fileSystem;
        private readonly Func<string, string> _getContentFolderFromPackageFolder;

        public PackageManifestReader(IFileSystem fileSystem, Func<string, string> getContentFolderFromPackageFolder)
        {
            _fileSystem = fileSystem;
            _getContentFolderFromPackageFolder = getContentFolderFromPackageFolder;
        }

        public IPackageInfo LoadFromFolder(string packageDirectory)
        {
            packageDirectory = packageDirectory.ToFullPath();

            var manifest = _fileSystem.LoadFromFile<PackageManifest>(packageDirectory, PackageManifest.FILE);
            var package = new PackageInfo(manifest.Name){
                Description = "{0} ({1})".ToFormat(manifest.Name, packageDirectory),
                Dependencies = manifest.Dependencies
            };

            

            // Right here, this needs to be different
            registerFolders(packageDirectory, package);

            var binPath = determineBinPath(packageDirectory);


            package.Role = manifest.Role;

            readAssemblyPaths(manifest, package, binPath);

            return package;
        }

        private string determineBinPath(string packageDirectory)
        {
            var binPath = FileSystem.Combine(packageDirectory, "bin");
            var debugPath = FileSystem.Combine(binPath, "debug");
            if (_fileSystem.DirectoryExists(debugPath))
            {
                binPath = debugPath;
            }
            return binPath;
        }

        private void registerFolders(string packageDirectory, PackageInfo package)
        {
            package.RegisterFolder(BottleFiles.WebContentFolder, _getContentFolderFromPackageFolder(packageDirectory));
            package.RegisterFolder(BottleFiles.DataFolder, FileSystem.Combine(packageDirectory, BottleFiles.DataFolder));
            package.RegisterFolder(BottleFiles.ConfigFolder, FileSystem.Combine(packageDirectory, BottleFiles.ConfigFolder));
        }

        private void readAssemblyPaths(PackageManifest manifest, PackageInfo package, string binPath)
        {
            var assemblyPaths = findCandidateAssemblyFiles(binPath);
            assemblyPaths.Each(path =>
            {
                var assemblyName = Path.GetFileNameWithoutExtension(path);
                if (manifest.Assemblies.Contains(assemblyName))
                {
                    package.RegisterAssemblyLocation(assemblyName, path);
                }
            });
        }

        private static IEnumerable<string> findCandidateAssemblyFiles(string binPath)
        {
            if (!Directory.Exists(binPath))
            {
                return new string[0];
            }

            return Directory.GetFiles(binPath).Where(IsPotentiallyAnAssembly);
        }

        public static bool IsPotentiallyAnAssembly(string file)
        {
            var extension = Path.GetExtension(file);
            return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".dll", StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return "Package Manifest Reader (Development Mode)";
        }
    }
}