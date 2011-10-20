using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bottles.Deployment.Configuration;
using Bottles.Deployment.Parsing;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class ReferenceBottleInput
    {
        [Description("The recipe that the host is in")]
        public string Recipe { get; set; }

        [Description("The host to add the reference to")]
        public string Host { get; set; }

        [Description("The name of the bottle to link")]
        public string Bottle { get; set; }

        [Description("Path to the deployment folder (~/deployment)")]
        public string DeploymentFlag { get; set; }
    }


    [CommandDescription("Adds a bottles reference to the specified host", Name = "ref-bottle")]
    public class ReferenceBottleCommand : FubuCommand<ReferenceBottleInput>
    {
        public override bool Execute(ReferenceBottleInput input)
        {
            var deploymentSettings = DeploymentSettings.ForDirectory(input.DeploymentFlag);

            var settings = new EnvironmentSettings();

            IFileSystem fileSystem = new FileSystem();

            Execute(input, settings, fileSystem, deploymentSettings);

            return true;
        }

        public void Execute(ReferenceBottleInput input, EnvironmentSettings settings, IFileSystem fileSystem, DeploymentSettings deploymentSettings)
        {
            string bottleText = "bottle:{0}".ToFormat(input.Bottle);


            var hostPath = deploymentSettings.GetHost(input.Recipe, input.Host);
            ConsoleWriter.Write("Analyzing the host file at " + input.Host);
            fileSystem.AlterFlatFile(hostPath, list =>
            {
                list.Fill(bottleText);
                list.Sort();

                ConsoleWriter.Write("Contents of file " + hostPath);
                list.Each(x => ConsoleWriter.Write("  " + x));
            });
        }


    }







}