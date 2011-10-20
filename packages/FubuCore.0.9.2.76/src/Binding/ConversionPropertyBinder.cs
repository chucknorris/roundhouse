using System;
using System.Reflection;
using FubuCore.Util;

namespace FubuCore.Binding
{
    public class ConversionPropertyBinder : IPropertyBinder
    {
        private readonly Cache<PropertyInfo, ValueConverter> _cache = new Cache<PropertyInfo, ValueConverter>();

        public ConversionPropertyBinder(IValueConverterRegistry converters)
        {
            _cache.OnMissing = prop => converters.FindConverter(prop);
        }

        public bool Matches(PropertyInfo property)
        {
            return _cache[property] != null;
        }

        // TODO -- need an integrated test with Connection String providers
        public void Bind(PropertyInfo property, IBindingContext context)
        {
            context.ForProperty(property, x =>
            {
                var converter = _cache[property];

                context.Logger.ChoseValueConverter(property, converter);

                var value = converter.Convert(x);
                    
                property.SetValue(x.Object, value, null);
            });
        }

    }
}