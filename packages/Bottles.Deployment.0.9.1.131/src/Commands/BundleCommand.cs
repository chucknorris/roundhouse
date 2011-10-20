using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Bottles.Deployment.Bootstrapping;
using Bottles.Deployment.Runtime;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class BundleInput : PlanInput
    {
        [Description("The directory name where the deployment artifacts are going to be written")]
        [RequiredUsage("imports", "default")]
        public string Destination { get; set; }

        [FlagAlias("create-bottles")]
        public bool CreateBottlesFlag { get; set; }
    }

    [Usage("default", "Bundle with only the environment settings in the deployment folder")]
    [Usage("imports", "Bundle with imported folders")]
    [CommandDescription("Bundles up designated deployment options to a folder")]
    public class BundleCommand : FubuCommand<BundleInput>
    {
        public override bool Execute(BundleInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            var options = input.CreateDeploymentOptions();
            settings.AddImportedFolders(options.ImportedFolders);

            if (input.CreateBottlesFlag)
            {
                createAllBottles(settings);
            }

            DeploymentBootstrapper
                .UsingService<IBundler>(settings, x => x.CreateBundle(input.Destination, options));

            return true;
        }

        private static void createAllBottles(DeploymentSettings settings)
        {
            var createAllCommand = new CreateAllCommand();
            var inputs = settings.Directories.Select(x => new CreateAllInput{
                DirectoryFlag = x
            });

            inputs.Each(x => createAllCommand.Execute(x));
        }
    }

    // TODO -- put integration test on this mess
}