using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bottles.Deployment.Configuration;
using Bottles.Deployment.Runtime;
using FubuCore;
using FubuCore.CommandLine;
using FubuCore.Configuration;

namespace Bottles.Deployment.Parsing
{
    public class DeploymentPlan
    {
        private readonly DeploymentGraph _graph;
        private readonly IEnumerable<HostManifest> _hosts;
        private readonly DeploymentOptions _options;
        private readonly IEnumerable<Recipe> _recipes;
        private SettingsData _rootData;

        public DeploymentPlan(DeploymentOptions options, DeploymentGraph graph)
        {
            _options = options;
            _graph = graph;
            _recipes = readRecipes();
            _hosts = collateHosts(_recipes);

            addProfileSettings();
            addEnvironmentSettings();

            readRoot();

            graph.Settings.Plan = this;
            graph.Settings.Environment = _graph.Environment;
            graph.Settings.Profile = _graph.Profile;


            options.Overrides.Each((key, value) =>
            {
                _graph.Profile.Data[key] = value;
            });
        }

        public void WriteToConsole()
        {
            ConsoleWriter.Line();
            ConsoleWriter.PrintHorizontalLine();
            ConsoleWriter.Write("Deploying profile {0}".ToFormat(Options.ProfileName));
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, "from deployment directory " + Settings.DeploymentDirectory);
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, "to target directory " + Settings.TargetDirectory);
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, "Applying recipe(s):  " + Recipes.Select(x => x.Name).Join(", "));
            ConsoleWriter.WriteWithIndent(ConsoleColor.Gray, 2, "Deploying host(s):  " + Hosts.Select(x => x.Name).Join(", "));
            ConsoleWriter.PrintHorizontalLine(); 
        }

        public string ProfileName
        {
            get
            {
                return _options.ProfileName;
            }
        }

        // TESTING CTOR ONLY!!!!!!!!
        public DeploymentPlan(DeploymentSettings settings, DeploymentOptions options, IEnumerable<Recipe> recipes, IEnumerable<HostManifest> hosts)
        {
            _recipes = recipes;
            settings.Plan = this;
            _hosts = hosts;
            _options = options;
            _rootData = new SettingsData();
        }

        public IEnumerable<SettingDataSource> GetSubstitutionDiagnosticReport()
        {
            var provider = SettingsProvider.For(Substitutions().ToArray());
            return provider.CreateDiagnosticReport(); 
        }

        public IEnumerable<SettingsData> Substitutions()
        {
            if (_rootData != null)
            {
                yield return _rootData;
            }

            yield return _graph.Environment.Data.SubsetByKey(key => !key.Contains("."));
            yield return _graph.Profile.Data.SubsetByKey(key => !key.Contains("."));
        }


        public DeploymentSettings Settings
        {
            get
            {
                return _graph.Settings;
            }
        }

        private void readRoot()
        {
            // Like to have some tracing about where ROOT comes from.  START HERE HERE HERE HERE HERE
            var requestData = SettingsRequestData.For(_graph.Profile.Data, _graph.Environment.Data);
            var valueExists = requestData.Value(EnvironmentSettings.ROOT, value =>
            {
                _graph.Settings.TargetDirectory = (string) value;
            });

            if (!valueExists)
            {
                _rootData = new SettingsData(SettingCategory.profile){
                    Provenance = typeof(DeploymentSettings).Name
                };
                _rootData.With(EnvironmentSettings.ROOT, _graph.Settings.TargetDirectory);
            }
        }

        private void addEnvironmentSettings()
        {
            _hosts.Each(host => host.RegisterSettings(_graph.Environment.DataForHost(host.Name)));
        }

        private void addProfileSettings()
        {
            _hosts.Each(host => host.RegisterSettings(_graph.Profile.DataForHost(host.Name)));
        }

        public DeploymentOptions Options
        {
            get { return _options; }
        }

        public IEnumerable<Recipe> Recipes
        {
            get { return _recipes; }
        }


        public IEnumerable<HostManifest> Hosts
        {
            get { return _hosts; }
        }

        public IEnumerable<string> BottleNames()
        {
            return _hosts.SelectMany(x => x.BottleReferences).Select(x => x.Name).Distinct();
        }

        private static IEnumerable<HostManifest> collateHosts(IEnumerable<Recipe> recipes)
        {
            if (recipes == null)
            {
                throw new Exception("Bah! no recipies");
            }


            var firstRecipe = recipes.First();
            recipes.Skip(1).Each(firstRecipe.AppendBehind);

            return firstRecipe.Hosts;
        }

        private IEnumerable<Recipe> readRecipes()
        {
            var recipes = buildEntireRecipeGraph(_graph.Recipes);

            // TODO -- log which recipes were selected
            recipes = new RecipeSorter().Order(recipes);
            return recipes;
        }

        private IEnumerable<Recipe> buildEntireRecipeGraph(IEnumerable<Recipe> allRecipesAvailable)
        {
            var recipesToRun = new List<string>();

            recipesToRun.AddRange(_graph.Profile.Recipes);
            
            //add on profile dependencies recipes
            recipesToRun.AddRange(dependentProfileRecipes());

            recipesToRun.AddRange(_options.RecipeNames);

            var dependencies = new List<string>();

            recipesToRun.Each(r =>
            {
                try
                {
                    var rec = allRecipesAvailable.Single(x => x.Name == r);
                    dependencies.AddRange(rec.Dependencies);
                }
                catch (Exception)
                {
                    var message = new StringBuilder();
                    message.AppendFormat("Couldn't find recipe '{0}'", r);
                    message.AppendLine();
                    message.AppendLine("Recipes found were:");
                    allRecipesAvailable.Each(rcp => message.AppendFormat("    {0}", rcp));
                    
                    message.AppendLine();
                    throw new Exception(message.ToString());
                }
            });

            recipesToRun.AddRange(dependencies.Distinct());

            return recipesToRun.Distinct().Select(name => allRecipesAvailable.Single(o => o.Name == name));
        }

        private IEnumerable<string> dependentProfileRecipes()
        {
            var x = _graph.Profile.ProfileDependencies.SelectMany(p =>
            {
                var profile = Profile.ReadFrom(Settings, p);
                return profile.Recipes;
            });
            return x;
        }

        public HostManifest GetHost(string hostName)
        {
            return Hosts.FirstOrDefault(x => x.Name == hostName);
        }

        public static DeploymentPlan Blank()
        {
            return new DeploymentPlan(new DeploymentSettings(), new DeploymentOptions(), new Recipe[0], new HostManifest[0]);
        }

        public void AssertAllRequiredValuesAreFilled()
        {
            var settingData = _hosts.SelectMany(x => x.AllSettingsData()).Union(Substitutions()).ToArray();
            SettingsProvider.For(settingData).AssertAllSubstitutionsCanBeResolved();
        }
    }



}