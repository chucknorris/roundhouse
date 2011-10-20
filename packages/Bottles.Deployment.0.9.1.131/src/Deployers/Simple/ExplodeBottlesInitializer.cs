using System.Collections.Generic;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;

namespace Bottles.Deployment.Deployers.Simple
{
    public class ExplodeBottlesInitializer : IInitializer<ExplodeBottles>
    {
        private readonly IBottleRepository _bottles;
        private readonly IFileSystem _fileSystem;

        public ExplodeBottlesInitializer(IFileSystem fileSystem, IBottleRepository bottles)
        {
            _fileSystem = fileSystem;
            _bottles = bottles;
        }

        public void Execute(ExplodeBottles directive, HostManifest host, IPackageLog log)
        {
            _fileSystem.DeleteDirectory(directive.RootDirectory);
            _fileSystem.CreateDirectory(directive.RootDirectory);

            host.BottleReferences.Each(x => explodeBottle(directive, log, x.Name));
        }

        public string GetDescription(ExplodeBottles directive)
        {
            return "Exploding bottles to " + directive.RootDirectory;
        }

        private void explodeBottle(ExplodeBottles directive, IPackageLog log, string bottleName)
        {
            _bottles.ExplodeFiles(new BottleExplosionRequest(log){
                BottleDirectory = BottleFiles.BinaryFolder,
                DestinationDirectory = directive.RootDirectory.AppendPath(directive.BinDirectory ?? string.Empty),
                BottleName = bottleName
            });

            _bottles.ExplodeFiles(new BottleExplosionRequest(log){
                BottleDirectory = BottleFiles.WebContentFolder,
                DestinationDirectory = directive.RootDirectory.AppendPath(directive.WebContentDirectory ?? string.Empty),
                BottleName = bottleName
            });

            _bottles.ExplodeFiles(new BottleExplosionRequest(log)
            {
                BottleDirectory = BottleFiles.DataFolder,
                DestinationDirectory = directive.RootDirectory.AppendPath(directive.DataDirectory ?? string.Empty),
                BottleName = bottleName
            });
        }
    }
}