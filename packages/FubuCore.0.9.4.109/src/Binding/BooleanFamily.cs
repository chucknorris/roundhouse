using System;
using System.ComponentModel;
using System.Reflection;

namespace FubuCore.Binding
{
    public class BooleanFamily : StatelessConverter
    {
        private static TypeConverter _converter = TypeDescriptor.GetConverter(typeof(bool));
        public const string CheckboxOn = "on";

        public override bool Matches(PropertyInfo property)
        {
            return property.PropertyType.IsTypeOrNullableOf<bool>();
        }

        public override object Convert(IPropertyContext context)
        {
            if (context.PropertyValue is bool) return context.PropertyValue;

            return context.PropertyValue.ToString().Contains(context.Property.Name)
            || context.PropertyValue.ToString().EqualsIgnoreCase(CheckboxOn)
            || (bool)_converter.ConvertFrom(context.PropertyValue);
        }
    }
}