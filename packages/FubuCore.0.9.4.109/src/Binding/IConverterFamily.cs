using System.Reflection;

namespace FubuCore.Binding
{
    public interface IConverterFamily
    {
        bool Matches(PropertyInfo property);
        ValueConverter Build(IValueConverterRegistry registry, PropertyInfo property);
    }
}