using System;
using FubuCore.Util;

namespace FubuCore.Binding
{
    public class ServiceArguments
    {
        private readonly Cache<Type, object> _children = new Cache<Type, object>();

        public T Get<T>() where T : class
        {
            return _children[typeof (T)] as T;
        }

        public void Set(Type pluginType, object arg)
        {
            _children[pluginType] = arg;
        }

        public bool Has(Type type)
        {
            return _children.Has(type);
        }

        public void EachService(Action<Type, object> action)
        {
            _children.Each(action);
        }

        public ServiceArguments With<T>(T @object)
        {
            _children[typeof (T)] = @object;
            return this;
        }
    }
}