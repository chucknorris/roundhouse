using System;
using System.Reflection;

namespace FubuCore.Binding
{
    public class PassthroughConverter<T> : StatelessConverter
    {
        public override bool Matches(PropertyInfo property)
        {
            return property.PropertyType.IsAssignableFrom(typeof (T));
        }

        public override object Convert(IPropertyContext context)
        {
            return context.PropertyValue;
        }
    }
}