using System;
using System.Reflection;
using FubuCore.Binding;

namespace FubuCore.Binding
{
    public class ASPNetObjectConversionFamily : StatelessConverter
    {
        public override bool Matches(PropertyInfo property)
        {
            return AggregateDictionary.IsSystemProperty(property);
        }

        public override object Convert(IPropertyContext context)
        {
            return context.PropertyValue;
        }
    }
}