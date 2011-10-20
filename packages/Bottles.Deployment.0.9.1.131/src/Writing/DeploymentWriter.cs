using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Bottles.Deployment.Commands;
using FubuCore;
using FubuCore.Reflection;
using FubuCore.Util;

namespace Bottles.Deployment.Writing
{
    public class DeploymentWriter
    {
        private readonly IList<PropertyValue> _profileValues = new List<PropertyValue>();

        private readonly Cache<string, RecipeDefinition> _recipes =
            new Cache<string, RecipeDefinition>(name => new RecipeDefinition(name));

        private readonly Cache<string, ProfileDefinition> _profiles = 
            new Cache<string, ProfileDefinition>(name => new ProfileDefinition(name));

        private readonly DeploymentSettings _settings;
        private readonly IFileSystem _system;
        private readonly TypeDescriptorCache _types = new TypeDescriptorCache();


        public DeploymentWriter(string destination) : this(destination, new FileSystem())
        {
        }

        public DeploymentWriter(string destination, IFileSystem system)
        {
            _settings = new DeploymentSettings(destination);
            _system = system;
        }

        public RecipeDefinition RecipeFor(string name)
        {
            return _recipes[name];
        }

        public ProfileDefinition ProfileFor(string name)
        {
            return _profiles[name];
        }

        public void Flush(FlushOptions options)
        {
            var input = new InitializeInput(_settings);
            if (options == FlushOptions.Wipeout)
            {
                input.ForceFlag = true;
            }

            new InitializeCommand().Execute(input);

            writeEnvironmentSettings();

            _recipes.Each(writeRecipe);

            _profiles.Each(writeProfile);
        }

        private void writeEnvironmentSettings()
        {
            _system.WriteToFlatFile(_settings.EnvironmentFile(),
                                    file => { _profileValues.Each(value => value.Write(file)); });
        }

        private void writeRecipe(RecipeDefinition recipe)
        {
            new RecipeWriter(_types).WriteTo(recipe, _settings);
        }

        private void writeProfile(ProfileDefinition profile)
        {
            new ProfileWriter().WriteTo(profile, _settings);
        }


        public void AddEnvironmentSetting<T>(Expression<Func<T, object>> property, string host, object value)
        {
            _profileValues.Add(new PropertyValue{
                Accessor = property.ToAccessor(),
                HostName = host,
                Value = value
            });
        }

        public void AddEnvironmentSetting(string name, object value)
        {
            _profileValues.Add(new PropertyValue{
                Name = name,
                Value = value
            });
        }
    }

    public enum FlushOptions
    {
        Preserve,
        Wipeout
    }
}