using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuCore
{
    public interface ITypeResolver
    {
        Type ResolveType(object model);
    }

    public class TypeResolver : ITypeResolver
    {
        private readonly IList<ITypeResolverStrategy> _strategies 
            = new List<ITypeResolverStrategy>();

        private readonly ITypeResolverStrategy _default = new DefaultStrategy();

        public Type ResolveType(object model)
        {
            if (model == null) return null;

            var strategy = _strategies.FirstOrDefault(x => x.Matches(model)) ?? _default;
            return strategy.ResolveType(model);
        }
         
        public void AddStrategy<T>() where T : ITypeResolverStrategy, new()
        {
            _strategies.Add(new T());
        }

        public class DefaultStrategy : ITypeResolverStrategy
        {
            public bool Matches(object model)
            {
                return true;
            }

            public Type ResolveType(object model)
            {
                return model.GetType();
            }
        }
    }

    

    public interface ITypeResolverStrategy
    {
        bool Matches(object model);
        Type ResolveType(object model);
    }
}