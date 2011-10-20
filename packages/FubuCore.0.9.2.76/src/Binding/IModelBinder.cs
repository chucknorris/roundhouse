using System;

namespace FubuCore.Binding
{
    public interface IModelBinder
    {
        bool Matches(Type type);
        void Bind(Type type, object instance, IBindingContext context);
        object Bind(Type type, IBindingContext context);
    }
}