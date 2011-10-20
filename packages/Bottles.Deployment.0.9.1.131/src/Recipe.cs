using System;
using System.Collections.Generic;
using System.Diagnostics;
using FubuCore.Util;

namespace Bottles.Deployment
{
    [DebuggerDisplay("Recipe:{_name}")]
    public class Recipe
    {
        private readonly string _name;
        private readonly Cache<string, HostManifest> _hosts = new Cache<string, HostManifest>(name => new HostManifest(name));
        private readonly List<string> _dependencies = new List<string>();

        public Recipe(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public HostManifest HostFor(string name)
        {
            return _hosts[name];
        }

        public IEnumerable<HostManifest> Hosts
        {
            get
            {
                return _hosts.GetAll();
            }
        }

        public IEnumerable<string> Dependencies
        {
            get { return _dependencies; }
        }

        public void RegisterDependency(string recipeName)
        {
            _dependencies.Add(recipeName);
        }

        public void RegisterHost(HostManifest host)
        {
            _hosts[host.Name] = host;
        }

        public void AppendBehind(Recipe recipe)
        {
            recipe.Hosts.Each(other => _hosts[other.Name].Append(other));
        }

        public bool Equals(Recipe other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other._name, _name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Recipe)) return false;
            return Equals((Recipe) obj);
        }

        public override int GetHashCode()
        {
            return (_name != null ? _name.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return _name;
        }
    }
}