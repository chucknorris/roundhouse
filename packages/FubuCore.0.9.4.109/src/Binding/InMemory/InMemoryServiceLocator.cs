using System;
using System.Collections.Generic;
using FubuCore.Conversion;
using FubuCore.Util;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore.Binding.InMemory
{
    public class InMemoryServiceLocator : ServiceLocatorImplBase
    {
        private readonly Cache<Type, object> _services = new Cache<Type, object>();

        public InMemoryServiceLocator()
        {
            Add<IObjectConverter>(new ObjectConverter(this, new ConverterLibrary()));
        }

        protected override object DoGetInstance(Type serviceType, string key)
        {
            return _services[serviceType];
        }

        public void Add<T>(T service)
        {
            _services[typeof (T)] = service;
        }

        protected override IEnumerable<object> DoGetAllInstances(Type serviceType)
        {
            throw new NotSupportedException();
        }
    }
}