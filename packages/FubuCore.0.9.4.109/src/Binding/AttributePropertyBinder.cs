using System.Reflection;
using FubuCore.Reflection;

namespace FubuCore.Binding
{
    public class AttributePropertyBinder : IPropertyBinder
    {
        public bool Matches(PropertyInfo property)
        {
            return property.HasAttribute<BindingAttribute>();
        }

        public void Bind(PropertyInfo property, IBindingContext context)
        {
            property.ForAttribute<BindingAttribute>(att => att.Bind(property, context));
        }
    }
}