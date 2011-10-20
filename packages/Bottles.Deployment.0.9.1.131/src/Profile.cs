using System;
using System.Collections.Generic;
using System.Text;
using Bottles.Deployment.Configuration;
using FubuCore;
using FubuCore.Configuration;

namespace Bottles.Deployment
{
    public class Profile : ProfileBase
    {
        public static readonly string RecipePrefix = "recipe:";
        public static string ProfileDependencyPrefix = "dependency:";
        private readonly IList<string> _recipes = new List<string>();
        private readonly IList<string> _profiles = new List<string>();

        public Profile(string profileName) : base(SettingCategory.profile, "Profile:  " + profileName)
        {
        }

        public IEnumerable<string> Recipes
        {
            get { return _recipes; }
        }

        public IEnumerable<string> ProfileDependencies
        {
            get { return _profiles; }
        }

        public static Profile ReadFrom(DeploymentSettings settings, string profileName)
        {
            var profile = new Profile(profileName);
            var profileFile = settings.ProfileFileNameFor(profileName);

            var fileSystem = new FileSystem();
            if (!fileSystem.FileExists(profileFile))
            {
                var sb = new StringBuilder();
                sb.AppendFormat("Couldn't find the profile '{0}'", profileFile);
                sb.AppendLine();
                sb.AppendLine("Looked in:");
                settings.Directories.Each(d => sb.AppendLine("  {0}".ToFormat(d)));

                throw new Exception(sb.ToString());
            }

            fileSystem.ReadTextFile(profileFile, profile.ReadText);

            return profile;
        }

        public void ReadText(string text)
        {
            if (text.IsEmpty()) return;

            if (text.StartsWith(RecipePrefix))
            {
                var recipeName = text.Substring(RecipePrefix.Length).Trim();
                AddRecipe(recipeName);
            }
            else if(text.StartsWith(ProfileDependencyPrefix))
            {
                var profileName = text.Substring(ProfileDependencyPrefix.Length).Trim();
                AddProfileDependency(profileName);
            }
            else
            {
                Data.Read(text);
            }
        }

        private void AddProfileDependency(string profileName)
        {
            _profiles.Fill(profileName);
        }

        public void AddRecipe(string recipe)
        {
            _recipes.Fill(recipe);
        }
    }
}