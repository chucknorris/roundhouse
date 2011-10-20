using System;
using FubuMVC.Core;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore.Binding
{
    public class ObjectResolver : IObjectResolver
    {
        private readonly IModelBinderCache _binders;
        private readonly IBindingLogger _logger;
        private readonly IServiceLocator _services;

        // Leave this here
        public ObjectResolver()
        {
        }

        public ObjectResolver(IServiceLocator services, IModelBinderCache binders, IBindingLogger logger)
        {
            _services = services;
            _binders = binders;
            _logger = logger;
        }

        public virtual BindResult BindModel(Type type, IRequestData data)
        {
            var context = new BindingContext(data, _services, _logger);
            return BindModel(type, context);
        }

        // Leave this virtual
        public virtual BindResult BindModel(Type type, IBindingContext context)
        {
            var binder = _binders.BinderFor(type);

            if (binder == null)
            {
                throw new FubuException(2200,
                    "Could not determine an IModelBinder for input type {0}. No model binders matched on this type. The standard model binder requires a parameterless constructor for the model type. Alternatively, you could implement your own IModelBinder which can process this model type.",
                    type.AssemblyQualifiedName);
            }

            context.Logger.ChoseModelBinder(type, binder);

            return executeModelBinder(type, binder, context);
        }

        public void TryBindModel(Type type, IBindingContext context, Action<BindResult> continuation)
        {
            var binder = _binders.BinderFor(type);

            if (binder != null)
            {
                var result = executeModelBinder(type, binder, context);
                continuation(result);
            }
        }

        private static BindResult executeModelBinder(Type type, IModelBinder binder, IBindingContext context)
        {
            try
            {
                return new BindResult{
                    Value = binder.Bind(type, context),
                    Problems = context.Problems
                };
            }
            catch (Exception e)
            {
                throw new FubuException(2201, e, "Fatal error while binding model of type {0}.  See inner exception",
                                        type.AssemblyQualifiedName);
            }
        }

        public static ObjectResolver Basic()
        {
            return new ObjectResolver(null, ModelBinderCache.Basic(), new NulloBindingLogger());
        }
    }
}