using System.Collections.Generic;
using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;
using roundhouse;
using roundhouse.consoles;
using roundhouse.folders;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.containers;
using roundhouse.infrastructure.filesystem;
using roundhouse.migrators;
using roundhouse.resolvers;
using roundhouse.runners;

namespace Bottles.Deployers.RoundhousE
{
    public class RoundhouseDeployer : IDeployer<Roundhouse>
    {
        private IBottleRepository _bottleRepository;

        public RoundhouseDeployer(IBottleRepository bottleRepository)
        {
            _bottleRepository = bottleRepository;
        }

        public void Execute(Roundhouse directive, HostManifest host, IPackageLog log)
        {
            var destinationDirectory = directive.GetDirectory().AppendPath("roundhouse");

            host.BottleReferences.Each(b =>
            {
                _bottleRepository.ExplodeFiles(new BottleExplosionRequest()
                                               {
                                                   BottleDirectory = BottleFiles.DataFolder,
                                                   CopyBehavior =  CopyBehavior.overwrite,
                                                   BottleName = b.Name,
                                                   DestinationDirectory = destinationDirectory
                                               });    
            });

            rh(directive, destinationDirectory);
        }

        private void rh(Roundhouse directive, string destinationDirectory)
        {
            var migrate = new Migrate();
            migrate.Set(cfg=>
            {
                cfg.ConnectionString = directive.ConnectionString;
                cfg.SqlFilesDirectory = destinationDirectory;
                cfg.Silent = true;
            }).Run();
        }

        public string GetDescription(Roundhouse directive)
        {
            return "Running Roundhouse (rh.exe) ";
        }

    }

    public class Roundhouse : IDirective
    {
        public string ConnectionString { get; set; }

        public string Directory { get; set; }
        public string GetDirectory()
        {
            return Directory ?? ".";
        }
    }
}
