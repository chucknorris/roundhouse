using System;

namespace FubuCore.Binding
{
    public interface IObjectResolver
    {
        BindResult BindModel(Type type, IRequestData data);
        BindResult BindModel(Type type, IBindingContext context);

        /// <summary>
        /// Use this method when the type may not have a matching IModelBinder
        /// </summary>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <param name="onResult"></param>
        void TryBindModel(Type type, IBindingContext context, Action<BindResult> continuation);
    }
}