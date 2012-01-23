using System;
using System.Collections.Generic;
using System.Reflection;

namespace FubuCore.Binding
{
    public class NestedObjectPropertyBinder : IPropertyBinder
    {
        public bool Matches(PropertyInfo property)
        {
            return true;
        }

        public void Bind(PropertyInfo property, IBindingContext context)
        {
            context.BindChild(property);
        }
    }
}