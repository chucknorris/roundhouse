using System.Reflection;

namespace FubuCore.Binding
{
    public interface IPropertyBinderCache
    {
        IPropertyBinder BinderFor(PropertyInfo property);
    }
}