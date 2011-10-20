using System;
using System.Collections.Generic;
using System.ComponentModel;
using Bottles.Deployment.Configuration;
using FubuCore;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{

    public class ReferenceRecipeInput
    {
        [Description("The name of the profile")]
        public string Profile { get; set; }

        [Description("The name of the recipe to reference")]
        public string Recipe { get; set; }

        [Description("Path to the deployment folder (~/deployment)")]
        public string DeploymentFlag { get; set; }


    }

    [CommandDescription("Adds a bottles reference to the specified host", Name="ref-recipe")]
    public class ReferenceRecipeCommand : FubuCommand<ReferenceRecipeInput>
    {
        public override bool Execute(ReferenceRecipeInput input)
        {
            var deploymentSettings = DeploymentSettings.ForDirectory(input.DeploymentFlag);

            var settings = new EnvironmentSettings();

            IFileSystem fileSystem = new FileSystem();

            Execute(input, settings, fileSystem, deploymentSettings);

            return true;
        }

        public void Execute(ReferenceRecipeInput input, EnvironmentSettings settings, IFileSystem fileSystem, DeploymentSettings deploymentSettings)
        {
            var recipeText = Profile.RecipePrefix + input.Recipe;
            var profileFile = deploymentSettings.ProfileFileNameFor(input.Profile);

            fileSystem.AlterFlatFile(profileFile, list =>
            {
                list.Fill(recipeText);
                list.Sort();

                ConsoleWriter.Write("Contents of file " + profileFile);
                list.Each(x => ConsoleWriter.Write("  " + x));
            });
        }

    }
}