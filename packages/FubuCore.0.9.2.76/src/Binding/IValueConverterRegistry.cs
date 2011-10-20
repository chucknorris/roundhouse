using System.Reflection;

namespace FubuCore.Binding
{
    public interface IValueConverterRegistry
    {
        ValueConverter FindConverter(PropertyInfo property);
    }
}