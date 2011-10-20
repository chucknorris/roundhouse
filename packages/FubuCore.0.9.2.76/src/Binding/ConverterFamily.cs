using System;
using System.Reflection;

namespace FubuCore.Binding
{
    public class ConverterFamily : IConverterFamily
    {
        private readonly Func<IValueConverterRegistry, PropertyInfo, ValueConverter> _builder;
        private readonly Predicate<PropertyInfo> _matches;

        public ConverterFamily(Predicate<PropertyInfo> matches, Func<IValueConverterRegistry, PropertyInfo, ValueConverter> builder)
        {
            _matches = matches;
            _builder = builder;
        }

        public bool Matches(PropertyInfo property)
        {
            return _matches(property);
        }

        public ValueConverter Build(IValueConverterRegistry registry, PropertyInfo property)
        {
            return _builder(registry, property);
        }
    }
}