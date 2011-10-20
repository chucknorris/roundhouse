using System;
using System.Reflection;
using FubuCore.Reflection;

namespace FubuCore.Binding
{
    public class MapFromWebPathFamily : StatelessConverter
    {
        public override bool Matches(PropertyInfo property)
        {
            return property.HasAttribute<MapFromWebPathAttribute>();
        }

        public override object Convert(IPropertyContext context)
        {
            var stringValue = context.PropertyValue as String;

            return stringValue.IsNotEmpty()
                       ? stringValue.ToAbsoluteUrl()
                       : stringValue;
        }
    }
}