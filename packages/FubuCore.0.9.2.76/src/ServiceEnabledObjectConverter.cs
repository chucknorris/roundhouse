using System;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore
{
    public class ServiceEnabledObjectConverter : ObjectConverter
    {
        private readonly IServiceLocator _locator;

        public ServiceEnabledObjectConverter(IServiceLocator locator)
        {
            _locator = locator;
        }

        public void RegisterConverter<T, TService>(Func<TService, string, T> converter)
        {
            RegisterConverter<T>(text => converter(_locator.GetInstance<TService>(), text));
        }

        public void RegisterConverterFamily<T>() where T : IObjectConverterFamily
        {
            var family = _locator.GetInstance<T>();
            RegisterConverterFamily(family);
        }
    }
}