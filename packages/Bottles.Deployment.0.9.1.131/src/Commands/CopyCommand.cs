using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Runtime;
using Bottles.Diagnostics;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    [CommandDescription("Copies all of the deployment structure to another folder with all the necessary bottle support", Name = "copy")]
    public class CopyCommand : FubuCommand<CopyInput>
    {
        public override bool Execute(CopyInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);

            LogWriter.Current.Header2("Copying deployment from {0} to {1}", settings.DeploymentDirectory, input.Destination);

            var system = new FileSystem();
            system.DeleteDirectory(input.Destination);
            system.CreateDirectory(input.Destination);


            var destinationDeploymentDirectory = input.Destination.AppendPath(ProfileFiles.DeploymentFolder);
            system.CopyToDirectory(settings.DeploymentDirectory, destinationDeploymentDirectory);
            system.DeleteDirectory(destinationDeploymentDirectory.AppendPath(ProfileFiles.TargetDirectory));

            DeploymentBootstrapper
                .UsingService<IBundler>(settings, x => x.ExplodeDeployerBottles(input.Destination));

            system.DeleteDirectory(settings.StagingDirectory);

            return true;
        }
    }
}