using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore;
using FubuCore.Util;
using StructureMap;


namespace Bottles.Deployment.Runtime
{
    public class DirectiveTypeRegistry : IDirectiveTypeRegistry
    {
        private readonly Cache<string, Type> _directiveTypes = new Cache<string, Type>();

        public DirectiveTypeRegistry(IContainer container)
        {
            container.Model.PluginTypes
                .Select(x => x.PluginType.FindInterfaceThatCloses(typeof(IDeploymentAction<>)))
                .Where(x => x != null).Each(type =>
                {
                    // TODO -- need to blow up if duplicate names are hit?
                    var directiveType = type.GetGenericArguments().First();
                    _directiveTypes[directiveType.Name] = directiveType;
                });
        }

        public Type DirectiveTypeFor(string name)
        {
            if(_directiveTypes.Has(name))
            {
                return _directiveTypes[name];
            }

            var description = _directiveTypes.GetAllKeys().Join(", ");

            throw new ArgumentException("Couldn't find type '{0}'\nAvailable directives are {1}".ToFormat(name, description));
        }


        public IEnumerable<Type> DirectiveTypes()
        {
            return _directiveTypes.GetAll();
        }


        public void AddType(Type type)
        {
            _directiveTypes[type.Name] = type;
        }
    }
}