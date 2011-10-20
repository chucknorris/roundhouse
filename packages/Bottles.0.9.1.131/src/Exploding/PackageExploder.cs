using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Bottles.Diagnostics;
using Bottles.Zipping;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Exploding
{
    public enum ExplodeOptions
    {
        DeleteDestination,
        PreserveDestination
    }

    public class ExplodeDirectory
    {
        public string PackageDirectory { get; set;}
        public string DestinationDirectory { get; set; }
        public IPackageLog Log { get; set; }

        public bool Equals(ExplodeDirectory other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.PackageDirectory, PackageDirectory) && Equals(other.DestinationDirectory, DestinationDirectory);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (ExplodeDirectory)) return false;
            return Equals((ExplodeDirectory) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((PackageDirectory != null ? PackageDirectory.GetHashCode() : 0)*397) ^ (DestinationDirectory != null ? DestinationDirectory.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("PackageDirectory: {0}, DestinationDirectory: {1}", PackageDirectory, DestinationDirectory);
        }
    }

    public class PackageExploder : IPackageExploder
    {
        public static PackageExploder GetPackageExploder(IFileSystem fileSystem)
        {
            return new PackageExploder(new ZipFileService(fileSystem), new PackageExploderLogger(text => LogWriter.Current.Trace(text)), fileSystem);
        }

        public static PackageExploder GetPackageExploder(IPackageLog log)
        {
            var fileSystem = new FileSystem();
            return new PackageExploder(new ZipFileService(fileSystem), new PackageExploderLogger(text => log.Trace(text)), fileSystem);
        }

        private readonly IFileSystem _fileSystem;
        private readonly IPackageExploderLogger _logger;
        private readonly IZipFileService _service;

        public PackageExploder(IZipFileService service, IPackageExploderLogger logger, IFileSystem fileSystem)
        {
            _service = service;
            _logger = logger;
            _fileSystem = fileSystem;
        }



        public IEnumerable<string> ExplodeAllZipsAndReturnPackageDirectories(string applicationDirectory, IPackageLog log)
        {
            LogWriter.Current.Trace("Exploding all the package zip files for the application at " + applicationDirectory);

            return ExplodeDirectory(new ExplodeDirectory(){
                DestinationDirectory = BottleFiles.GetExplodedPackagesDirectory(applicationDirectory),
                PackageDirectory = BottleFiles.GetApplicationPackagesDirectory(applicationDirectory),
                Log = log
            });
        }


        public IEnumerable<string> ExplodeDirectory(ExplodeDirectory directory)
        {
            string packageFolder = directory.PackageDirectory;
            var fileSet = new FileSet
                          {
                              Include = "*.zip"
                          };

            directory.Log.Trace("Searching for zip files in package directory " + packageFolder);

            var packageFileNames = _fileSystem.FileNamesFor(fileSet, packageFolder);

            return packageFileNames.Select(file =>
            {
                var packageName = Path.GetFileNameWithoutExtension(file);
                var explodedDirectory = FileSystem.Combine(directory.DestinationDirectory, packageName);

                // TODO -- need more logging here. Pass in the log and have it log what happens internally
                var request = new ExplodeRequest{
                    Directory = explodedDirectory,
                    ExplodeAction = () => Explode(file, explodedDirectory, ExplodeOptions.DeleteDestination),
                    GetVersion = () => _service.GetVersion(file),
                    LogSameVersion = () => _logger.WritePackageZipFileWasSameVersionAsExploded(file)
                };


                explode(request);

                return explodedDirectory;
            }).ToList();  // Needs to be evaluated right now.
        }

        

        //destinationDirectory = var directoryName = BottleFiles.DirectoryForPackageZipFile(applicationDirectory, sourceZipFile);
        public void Explode(string sourceZipFile, string destinationDirectory, ExplodeOptions options)
        {
            _logger.WritePackageZipFileExploded(sourceZipFile, destinationDirectory);
            _service.ExtractTo(sourceZipFile, destinationDirectory, options);
        }

        public void CleanAll(string applicationDirectory)
        {
            ConsoleWriter.Write("Cleaning all exploded packages out of " + applicationDirectory);
            var directory = BottleFiles.GetExplodedPackagesDirectory(applicationDirectory);
            clearExplodedDirectories(directory);

            // This is here for legacy installations that may have old exploded packages in bin/fubu-packages
            clearExplodedDirectories(BottleFiles.GetApplicationPackagesDirectory(applicationDirectory));
        }

        private void clearExplodedDirectories(string directory)
        {
            _fileSystem.ChildDirectoriesFor(directory).Each(x =>
            {
                _logger.WritePackageDirectoryDeleted(x);
                _fileSystem.DeleteDirectory(x);
            });
        }

        public string ReadVersion(string directoryName)
        {
            var parts = new[]{
                directoryName,
                BottleFiles.VersionFile
            };

            // TODO -- harden?
            if (_fileSystem.FileExists(parts))
            {
                return _fileSystem.ReadStringFromFile(parts);
            }

            return Guid.Empty.ToString();
        }

        public void ExplodeAssembly(string applicationDirectory, Assembly assembly, IPackageFiles files)
        {
            var directory = BottleFiles.GetDirectoryForExplodedPackage(applicationDirectory, assembly.GetName().Name);

            var request = new ExplodeRequest{
                Directory = directory,
                GetVersion = () => assembly.GetName().Version.ToString(),
                LogSameVersion =
                    () =>
                    ConsoleWriter.Write(
                        "Assembly {0} has already been 'exploded' onto disk".ToFormat(assembly.GetName().FullName)),
                ExplodeAction = () => explodeAssembly(assembly, directory)
            };

            explode(request);

            _fileSystem.ChildDirectoriesFor(directory).Each(child =>
            {
                var name = Path.GetFileName(child);

                files.RegisterFolder(name, child.ToFullPath());
            });
        }

        private void explodeAssembly(Assembly assembly, string directory)
        {
            _fileSystem.DeleteDirectory(directory);
            _fileSystem.CreateDirectory(directory);

            assembly.GetManifestResourceNames().Where(BottleFiles.IsEmbeddedPackageZipFile).Each(name =>
            {
                var folderName = BottleFiles.EmbeddedPackageFolderName(name);
                var stream = assembly.GetManifestResourceStream(name);

                var description = "Resource {0} in Assembly {1}".ToFormat(name, assembly.GetName().FullName);
                var destinationFolder = FileSystem.Combine(directory, folderName);

                _service.ExtractTo(description, stream, destinationFolder);

                var version = assembly.GetName().Version.ToString();
                _fileSystem.WriteStringToFile(FileSystem.Combine(directory, BottleFiles.VersionFile), version);
            });
        }




        private void explode(ExplodeRequest request)
        {
            if (_fileSystem.DirectoryExists(request.Directory))
            {
                var packageVersion = request.GetVersion();
                var folderVersion = ReadVersion(request.Directory);

                if (packageVersion == folderVersion)
                {
                    request.LogSameVersion();
                    return;
                }
            }

            request.ExplodeAction();
        }




        #region Nested type: ExplodeRequest

        public class ExplodeRequest
        {
            public Func<string> GetVersion { get; set; }
            public string Directory { get; set; }
            public Action ExplodeAction { get; set; }
            public Action LogSameVersion { get; set; }
        }

        #endregion
    }

    
}