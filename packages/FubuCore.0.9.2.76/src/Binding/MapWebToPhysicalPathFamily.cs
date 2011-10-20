using System;
using System.Reflection;
using FubuCore.Reflection;

namespace FubuCore.Binding
{
    public class MapWebToPhysicalPathFamily : StatelessConverter
    {
        public override bool Matches(PropertyInfo property)
        {
            return property.HasAttribute<MapWebToPhysicalPathAttribute>();
        }

        public override object Convert(IPropertyContext context)
        {
            var strVal = context.PropertyValue as String;

            return strVal.IsNotEmpty()
                       ? strVal.ToPhysicalPath()
                       : strVal;
        }
    }
}