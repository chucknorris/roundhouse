using System;
using System.Reflection;
using FubuCore.Reflection;

namespace FubuCore.Binding
{
    public class ExpandEnvironmentVariablesFamily : StatelessConverter
    {
        public override bool Matches(PropertyInfo property)
        {
            return property.HasAttribute<ExpandEnvironmentVariablesAttribute>();
        }

        public override object Convert(IPropertyContext context)
        {
            var strVal = context.PropertyValue as String;

            return strVal.IsNotEmpty()
                       ? Environment.ExpandEnvironmentVariables(strVal)
                       : strVal;
        }
    }
}