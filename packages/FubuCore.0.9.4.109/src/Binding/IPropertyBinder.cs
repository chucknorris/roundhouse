using System.Reflection;

namespace FubuCore.Binding
{
    public interface IPropertyBinder
    {
        bool Matches(PropertyInfo property);
        void Bind(PropertyInfo property, IBindingContext context);
    }
}