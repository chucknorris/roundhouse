using System.Collections.Generic;
using Bottles.Deployment;
using Bottles.Deployment.Runtime;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;
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
            host.BottleReferences.Each(b =>
            {
                var destinationDirectory = directive.GetDirectory().AppendPath(b.Name);
                _bottleRepository.ExplodeFiles(new BottleExplosionRequest()
                                               {
                                                   BottleDirectory = BottleFiles.DataFolder,
                                                   CopyBehavior =  CopyBehavior.overwrite,
                                                   BottleName = b.Name,
                                                   DestinationDirectory = destinationDirectory
                                               });

                rh(directive, destinationDirectory.AppendPath("roundhouse"));
            });

        }

        private void rh(Roundhouse directive, string destinationDirectory)
        {
            var configuration = new DefaultConfiguration();
            ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
            ApplicationConfiguraton.build_the_container(configuration);
            
            configuration.ConnectionString = directive.ConnectionString;
            configuration.SqlFilesDirectory = destinationDirectory;


            RoundhouseMigrationRunner runner = new RoundhouseMigrationRunner(
                configuration.RepositoryPath,
                Container.get_an_instance_of<roundhouse.environments.Environment>(),
                Container.get_an_instance_of<KnownFolders>(),
                Container.get_an_instance_of<FileSystemAccess>(),
                Container.get_an_instance_of<DatabaseMigrator>(),
                Container.get_an_instance_of<VersionResolver>(),
                configuration.Silent,
                configuration.Drop,
                configuration.DoNotCreateDatabase,
                configuration.WithTransaction,
                configuration.RecoveryModeSimple,
                configuration);


            runner.run();
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
