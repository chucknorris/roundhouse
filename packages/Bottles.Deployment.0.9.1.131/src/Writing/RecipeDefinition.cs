using System.Collections.Generic;
using FubuCore.Util;

namespace Bottles.Deployment.Writing
{
    public class RecipeDefinition
    {
        private readonly string _name;
        private readonly Cache<string, HostDefinition> _hosts = new Cache<string, HostDefinition>(name => new HostDefinition(name));
        private readonly List<string> _dependencies = new List<string>();

        public RecipeDefinition(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public HostDefinition HostFor(string name)
        {
            return _hosts[name];
        }

        public void RegisterDependency(string recipeName)
        {
            _dependencies.Add(recipeName);
        }

        public IEnumerable<string> Dependencies
        {
            get { return _dependencies; }
        }

        public IEnumerable<HostDefinition> Hosts()
        {
            return _hosts;
        }
    }
}