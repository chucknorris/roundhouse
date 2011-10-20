using System.Collections.Generic;
using System.IO;
using System.Linq;
using FubuCore;

namespace Bottles.Deployment.Parsing
{
    public class RecipeReader
    {
        public static Recipe ReadFrom(string recipeDirectory)
        {
            var recipeName = Path.GetFileName(recipeDirectory);
            var recipe = new Recipe(recipeName);
            var fileSystem = new FileSystem();

            fileSystem.ReadTextFile(FileSystem.Combine(recipeDirectory, ProfileFiles.RecipesControlFile), s =>
                {
                    //TODO: Harden this for bad syntax
                    var parts = s.Split(':');
                    recipe.RegisterDependency(parts[1]);
                });


            fileSystem.FindFiles(recipeDirectory, new FileSet(){
                Include = "*.host"
            }).Each(file =>
            {
                var host = HostReader.ReadFrom(file);
                recipe.RegisterHost(host);
            });

            return recipe;
        }

        public static IEnumerable<Recipe> ReadRecipes(string recipesDir)
        {
            return Directory.GetDirectories(recipesDir).Select(ReadFrom).ToList();
        }
    }
}