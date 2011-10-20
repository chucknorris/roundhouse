using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bottles.Creation;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles
{
    public static class FileSystemExtensions
    {

        public static bool PackageManifestExists(this IFileSystem fileSystem, string directory)
        {
            return fileSystem.FileExists(directory, PackageManifest.FILE);
        }

        public static PackageManifest LoadPackageManifestFrom(this IFileSystem fileSystem, string directory)
        {
            return fileSystem.TryFindManifest(directory, PackageManifest.FILE)
                   ?? fileSystem.TryFindManifest(directory, PackageManifest.FILE);
        }

        public static PackageManifest TryFindManifest(this IFileSystem system, string directory, string fileName)
        {
            if (fileName.IsEmpty()) return null;

            var path = FileSystem.Combine(directory, fileName);
            if (system.FileExists(path))
            {
                var manifest = system.LoadFromFile<PackageManifest>(path);
                manifest.ManifestFileName = path;

                return manifest;
            }

            return null;
        }


        public static LinkManifest LoadLinkManifestFrom(this IFileSystem fileSystem, string folder)
        {
            return fileSystem.LoadFromFile<LinkManifest>(folder, LinkManifest.FILE);
        }


        public static bool LinkManifestExists(this IFileSystem fileSystem, string directory)
        {
            return fileSystem.FileExists(directory, LinkManifest.FILE);
        }





        public static string FindBinaryDirectory(this IFileSystem fileSystem, string directory, CompileTargetEnum target)
        {
            var binFolder = directory.AppendPath("bin");
            var compileTargetFolder = binFolder.AppendPath(target.ToString());
            if (fileSystem.DirectoryExists(compileTargetFolder))
            {
                binFolder = compileTargetFolder;
            }
            else
            {
                LogWriter.Current.Trace("'{0}' did not exist.", compileTargetFolder);
            }

            LogWriter.Current.Trace("  Looking for binaries at " + binFolder);

            return binFolder;
        }

		public static IEnumerable<string> FindAssemblyNames(this IFileSystem fileSystem, string directory)
        {
            var fileSet = new FileSet{
                DeepSearch = false,
                Include = "*.dll;*.exe"
            };

            return fileSystem.FindFiles(directory, fileSet).Select(Path.GetFileNameWithoutExtension);
        }


    }
}