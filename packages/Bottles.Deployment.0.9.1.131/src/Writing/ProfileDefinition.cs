using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FubuCore.Reflection;

namespace Bottles.Deployment.Writing
{
    public class ProfileDefinition
    {
        private readonly string _name;
        private readonly IList<string> _recipes = new List<string>();
        private readonly IList<string> _profileDependencies = new List<string>();
        private readonly IList<PropertyValue> _values = new List<PropertyValue>();

        public ProfileDefinition(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public void AddProfileDependency(string profile)
        {
            _profileDependencies.Fill(profile);
        }
        public void AddRecipe(string recipe)
        {
            _recipes.Fill(recipe);            
        }

        public IEnumerable<string> Recipes
        {
            get { return _recipes; }
        }

        public IEnumerable<string> ProfileDependencies
        {
            get { return _profileDependencies; }
        }

        public void AddProperty<T>(Expression<Func<T, object>> expression, object value)
        {
            _values.Add(new PropertyValue()
                        {
                            Accessor = expression.ToAccessor(),
                            Value = value
                        });
        }

        public void AddProperty<T>(Expression<Func<T, object>> expression, string host, object value)
        {
            _values.Add(new PropertyValue()
                        {
                            Accessor = expression.ToAccessor(),
                            Value = value,
                            HostName = host
                        });
        }

        public void AddProperty(string key, string value)
        {
            _values.Add(new PropertyValue(){
                Name = key,
                Value = value
            });
        }

        public IEnumerable<PropertyValue> Values
        {
            get { return _values; }
        }
    }
}