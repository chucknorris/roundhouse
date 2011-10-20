using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace FubuCore.Binding
{
    public class NumericTypeFamily : StatelessConverter
    {
        public override bool Matches(PropertyInfo property)
        {
            return property.PropertyType.IsNumeric();
        }

        public override object Convert(IPropertyContext context)
        {
            Type propertyType = context.Property.PropertyType;

            var converter = TypeDescriptor.GetConverter(propertyType);

            if (context.PropertyValue != null)
            {
                if (context.PropertyValue.GetType() == propertyType)
                {
                    return context.PropertyValue;
                }
                if (context.PropertyValue.ToString().IsValidNumber())
                {
                    var valueToConvert = removeNumericGroupSeparator(context.PropertyValue.ToString());
                    return converter.ConvertFrom(valueToConvert);
                }
            }

            return converter.ConvertFrom(context.PropertyValue);
        }

        private static string removeNumericGroupSeparator(string value)
        {
            var culture = Thread.CurrentThread.CurrentCulture;
            var numberSeparator = culture.NumberFormat.NumberGroupSeparator;
            return value.Replace(numberSeparator, "");
        }
    }
}