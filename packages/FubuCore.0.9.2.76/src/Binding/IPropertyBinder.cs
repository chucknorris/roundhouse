using System;
using System.Reflection;

namespace FubuCore.Binding
{
    public interface IPropertyBinder
    {
        bool Matches(PropertyInfo property);
        void Bind(PropertyInfo property, IBindingContext context);
    }

    public class IgnorePropertyBinder : IPropertyBinder
    {
        private readonly Func<PropertyInfo, bool> _filter;

        public IgnorePropertyBinder(Func<PropertyInfo, bool> filter)
        {
            _filter = filter;
        }

        public bool Matches(PropertyInfo property)
        {
            return _filter(property);
        }

        public void Bind(PropertyInfo property, IBindingContext context)
        {
            // no-op
        }
    }
}