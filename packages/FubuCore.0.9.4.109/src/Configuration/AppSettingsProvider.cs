using System;
using FubuCore.Binding;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore.Configuration
{
    public class AppSettingsProvider : ISettingsProvider
    {
        private readonly IServiceLocator _locator;
        private readonly IObjectResolver _resolver;

        public AppSettingsProvider(IObjectResolver resolver, IServiceLocator locator)
        {
            _resolver = resolver;
            _locator = locator;
        }

        public T SettingsFor<T>() where T : class, new()
        {
            Type settingsType = typeof (T);

            object value = SettingsFor(settingsType);

            return (T) value;
        }

        public object SettingsFor(Type settingsType)
        {
            IBindingContext context = new BindingContext(new AppSettingsRequestData(), _locator, new NulloBindingLogger())
                .PrefixWith(settingsType.Name + ".");

            BindResult result = _resolver.BindModel(settingsType, context);

            result.AssertNoProblems(settingsType);

            return result.Value;
        }


    }
}