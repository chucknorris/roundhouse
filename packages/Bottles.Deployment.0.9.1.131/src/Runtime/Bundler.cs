using System;
using Bottles.Deployment.Parsing;
using Bottles.Deployment.Runtime.Content;
using Bottles.Diagnostics;
using FubuCore;
using System.Collections.Generic;

namespace Bottles.Deployment.Runtime
{
    public interface IBundler
    {
        void CreateBundle(string destination, DeploymentOptions options);
        void ExplodeDeployerBottles(string destination);
    }

    public class Bundler : IBundler
    {
        private readonly IFileSystem _system;
        private readonly IDeploymentController _controller;
        private readonly DeploymentSettings _settings;
        private readonly IBottleRepository _bottles;

        public Bundler(IFileSystem system, IDeploymentController controller, DeploymentSettings settings, IBottleRepository bottles)
        {
            _system = system;
            _controller = controller;
            _settings = settings;
            _bottles = bottles;
        }

        public void CreateBundle(string destination, DeploymentOptions options)
        {
            var plan = _controller.BuildPlan(options);
            CreateBundle(destination, plan);
        }

        // TODO -- want an end to end test on this mess
        public virtual void CreateBundle(string destination, DeploymentPlan plan)
        {
            var destinationSettings = createDestination(destination);

            var copier = new DeploymentFileCopier(_system, _settings, destinationSettings);
            copyFiles(copier, plan);

            // Need to explode the bottles zip too
            ExplodeDeployerBottles(destination);

            _system.DeleteDirectory(_settings.StagingDirectory);
        }

        public void ExplodeDeployerBottles(string destination)
        {
            _settings.DeployerBottleNames().Each(name =>
            {
                var request = new BottleExplosionRequest(){
                    BottleDirectory = BottleFiles.BinaryFolder,
                    BottleName = name,
                    DestinationDirectory = destination
                };

                _bottles.ExplodeFiles(request);
            });
        }

        private DeploymentSettings createDestination(string destination)
        {
            return LogWriter.Current.Indent(() =>
            {
                LogWriter.Current.Header2("Creating directory " + destination);

                var destinationSettings = new DeploymentSettings(destination.AppendPath(ProfileFiles.DeploymentFolder));
                _system.DeleteDirectory(destination);
                _system.CreateDirectory(destination);
                return destinationSettings;
            });


        }

        private static void copyFiles(DeploymentFileCopier copier, DeploymentPlan plan)
        {
            LogWriter.Current.Indent(() =>
            {
                copier.CopyFile(x => x.EnvironmentFile());
                copier.CopyFile(x => x.ProfileFileNameFor(plan.ProfileName));

                plan.BottleNames().Each(name => copier.CopyFile(x => x.BottleFileFor(name)));

                plan.Recipes.Each(r => copier.CopyFile(x => x.GetRecipeDirectory(r.Name)));
            }); 
        }
    }

    public interface IDeploymentFileCopier
    {
        void CopyFile(Func<DeploymentSettings, string> pathSource);
    }

    public class DeploymentFileCopier : IDeploymentFileCopier
    {
        private readonly IFileSystem _system;
        private readonly DeploymentSettings _source;
        private readonly DeploymentSettings _destination;

        public DeploymentFileCopier(IFileSystem system, DeploymentSettings source, DeploymentSettings destination)
        {
            _system = system;
            _source = source;
            _destination = destination;
        }

        public void CopyFile(Func<DeploymentSettings, string> pathSource)
        {
            var source = pathSource(_source);
            var destination = pathSource(_destination);


            LogWriter.Current.Trace("Copying {0} to {1}", source, destination);
            _system.Copy(source, destination);
        }
    }
}