using System;
using FubuCore.Binding;

namespace FubuCore.Binding
{
    public interface IModelBinderCache
    {
        IModelBinder BinderFor(Type modelType);
    }
}