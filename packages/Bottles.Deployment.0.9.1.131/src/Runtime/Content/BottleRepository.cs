using System;
using System.IO;
using Bottles.Exploding;
using Bottles.Zipping;
using FubuCore;
using System.Collections.Generic;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Runtime.Content
{
    public class BottleRepository : IBottleRepository
    {
        private readonly IFileSystem _fileSystem;
        private readonly IZipFileService _zipService;
        private readonly DeploymentSettings _settings;

        public BottleRepository(IFileSystem fileSystem, IZipFileService zipService, DeploymentSettings settings)
        {
            _fileSystem = fileSystem;
            _zipService = zipService;
            _settings = settings;
        }

        public virtual void CopyTo(string bottleName, string destination)
        {
            var path = _settings.BottleFileFor(bottleName);
            _fileSystem.CopyToDirectory(path, destination);
        }

        public void ExplodeTo(string bottleName, string destination)
        {
            var bottleFile = _settings.BottleFileFor(bottleName);

            // TODO -- needs logging?
            //REVIEW: get_app_dir, zip-filename == path???
            _zipService.ExtractTo(bottleFile, destination, ExplodeOptions.PreserveDestination);
        }

        public void ExplodeFiles(BottleExplosionRequest request)
        {
            var bottleName = request.BottleName;

            if (request.BottleDirectory.IsEmpty())
            {
                CopyTo(request.BottleName, request.DestinationDirectory);
                return;
            }

            var bottleFile = _settings.BottleFileFor(bottleName);
            _fileSystem.CreateDirectory(_settings.StagingDirectory);

            var tempDirectory = _settings.StagingDirectory.AppendPath(bottleName);


            explodeToStaging(request, bottleFile, tempDirectory);

            var sourceDirectory = tempDirectory.AppendPath(request.BottleDirectory);

            _fileSystem.CreateDirectory(request.DestinationDirectory);

            var files = _fileSystem.FindFiles(sourceDirectory, new FileSet
                                                   {
                DeepSearch = true,
                Include = "*.*"
            });


            request.Log.Trace("Copying directory '{0}' to '{1}'", sourceDirectory, request.DestinationDirectory);
            files.Each(file =>
            {
                var destinationFile = request.DestinationDirectory.AppendPath(file.PathRelativeTo(sourceDirectory));
                if(request.DetailedLogging)
                {
                    request.Log.Trace("Copying {0} to {1}", file, destinationFile);
                }
                
                
                _fileSystem.Copy(file, destinationFile, request.CopyBehavior);
            });
        }

        public PackageManifest ReadManifest(string bottleName)
        {
            var fileName = _settings.BottleFileFor(bottleName);
            return _zipService.GetPackageManifest(fileName);
        }

        public void AssertAllBottlesExist(IEnumerable<string> names)
        {
            var writer = new StringWriter();
            names.Each(name =>
            {
                var file = _settings.BottleFileFor(name);
                if (!_fileSystem.FileExists(file))
                {
                    writer.WriteLine("Cannot find a bottle for for " + name);
                }
            });

            if (writer.ToString().IsNotEmpty())
            {
                
                throw new ApplicationException(writer.ToString());
            }
        }

        private readonly IList<string> _bottlesExplodedToStaging = new List<string>();
        private void explodeToStaging(BottleExplosionRequest request, string bottleFile, string tempDirectory)
        {
            if (_bottlesExplodedToStaging.Contains(request.BottleName))
            {
                return;
            }

            request.Log.Trace("Exploding bottle {0} to {1}", bottleFile, tempDirectory);
            _zipService.ExtractTo(bottleFile, tempDirectory, ExplodeOptions.DeleteDestination);
        
            _bottlesExplodedToStaging.Add(request.BottleName);
        }


    }
}