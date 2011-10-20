using System.Collections.Generic;
using System.ComponentModel;
using Bottles.Deployment.Writing;
using FubuCore.CommandLine;

namespace Bottles.Deployment.Commands
{
    public class AddProfileInput
    {
        [Description("The name of the profile")]
        public string Name { get; set; }

        [Description("List of recipes to include")]
        public IEnumerable<string> Recipes { get; set; }

        [Description("Where the ~/deployment folder is")]
        public string DeploymentFlag { get; set; }
    }

    [CommandDescription("Adds a deployment profile", Name="add-profile")]
    public class AddProfileCommand : FubuCommand<AddProfileInput>
    {
        public override bool Execute(AddProfileInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);
            var profile = new ProfileDefinition(input.Name);
            if (input.Recipes != null)
            {
                input.Recipes.Each(profile.AddRecipe);
            }
            var writer = new ProfileWriter();
            writer.WriteTo(profile, settings);
            return true;
        }
    }
}