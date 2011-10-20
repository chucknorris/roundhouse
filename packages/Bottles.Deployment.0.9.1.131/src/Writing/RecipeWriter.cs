using System.Collections.Generic;
using FubuCore;
using FubuCore.Reflection;

namespace Bottles.Deployment.Writing
{
    public class RecipeWriter
    {
        private readonly IFileSystem _fileSystem = new FileSystem();
        private readonly TypeDescriptorCache _types;

        public RecipeWriter(TypeDescriptorCache types)
        {
            _types = types;
        }

        public void WriteTo(RecipeDefinition recipe, DeploymentSettings settings)
        {
            var recipeDirectory = settings.GetRecipeDirectory(recipe.Name);

            _fileSystem.CreateDirectory(recipeDirectory);

            // TODO -- get this into DeploymentSettings
            var controlFilePath = FileSystem.Combine(recipeDirectory, ProfileFiles.RecipesControlFile);

            new FileSystem().WriteToFlatFile(controlFilePath, writer =>
            {
                recipe.Dependencies.Each(d =>
                {
                    var line = "dependency:{0}".ToFormat(d);
                    writer.WriteLine(line);
                });
            });

            recipe.Hosts().Each(host => new HostWriter(_types).WriteTo(recipe.Name, host, settings));
        }
    }
}