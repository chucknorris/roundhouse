using Bottles.Deployment.Configuration;
using Bottles.Deployment.Runtime;
using System.Linq;
using FubuCore;

namespace Bottles.Deployment.Parsing
{
    public interface IDeploymentGraphReader
    {
        DeploymentGraph Read(DeploymentOptions options);
    }

    public class DeploymentGraphReader : IDeploymentGraphReader
    {
        private readonly DeploymentSettings _settings;

        public DeploymentGraphReader(DeploymentSettings settings)
        {
            _settings = settings;
        }

        public DeploymentGraph Read(DeploymentOptions options)
        {
            _settings.AddImportedFolders(options.ImportedFolders);

            var allRecipes = _settings.Directories.Select(x => x.AppendPath(ProfileFiles.RecipesDirectory)).SelectMany(RecipeReader.ReadRecipes);
            return new DeploymentGraph{
                Environment = EnvironmentSettings.ReadFrom(_settings.EnvironmentFile()),
                Profile = Profile.ReadFrom(_settings, options.ProfileName),
                Recipes = allRecipes,
                Settings = _settings
            };
        }
    }
}