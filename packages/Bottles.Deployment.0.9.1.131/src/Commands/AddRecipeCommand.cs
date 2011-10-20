using System.ComponentModel;
using Bottles.Deployment.Writing;
using FubuCore.CommandLine;
using FubuCore;
using FubuCore.Reflection;

namespace Bottles.Deployment.Commands
{
    public class AddRecipeInput
    {
        [Description("The name of the recipe")]
        public string Name { get; set; }

        [Description("Where the ~/deployment folder is")]
        public string DeploymentFlag { get; set; }
    }

    [CommandDescription("Adds a recipe to a deployment", Name="add-recipe")]
    public class AddRecipeCommand : FubuCommand<AddRecipeInput>
    {
        public override bool Execute(AddRecipeInput input)
        {
            var settings = DeploymentSettings.ForDirectory(input.DeploymentFlag);

            var recipe = new RecipeDefinition(input.Name);
            
            var writer = new RecipeWriter(new TypeDescriptorCache());
            writer.WriteTo(recipe, settings);


            return true;
        }
    }
}