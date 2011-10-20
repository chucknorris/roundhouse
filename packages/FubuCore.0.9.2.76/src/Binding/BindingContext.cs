using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuMVC.Core;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore.Binding
{
    public class BindingContext : IBindingContext, IPropertyContext
    {
        private static readonly List<Func<string, string>> _namingStrategies;
        private readonly IServiceLocator _locator;
        private readonly IBindingLogger _logger;
        private readonly IRequestData _requestData;
        private readonly IList<ConvertProblem> _problems = new List<ConvertProblem>();
        private readonly Lazy<ISmartRequest> _request;

        static BindingContext()
        {
            _namingStrategies = new List<Func<string, string>>
            {
                p => p,
                p => p.Replace("_", "-"),
                p => "[{0}]".ToFormat(p)  // This was necessary 

            };
        }

        public BindingContext(IRequestData requestData, IServiceLocator locator, IBindingLogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");

            _requestData = requestData;
            _locator = locator;
            _logger = logger;

            _request = new Lazy<ISmartRequest>(() => new SmartRequest(_requestData, _locator.GetInstance<IObjectConverter>()));
        }

        public IBindingLogger Logger
        {
            get { return _logger; }
        }

        public IList<ConvertProblem> Problems
        {
            get { return _problems; } 
        }

        public T Service<T>()
        {
            return _locator.GetInstance<T>();
        }

        public object Service(Type typeToFind)
        {
            return _locator.GetInstance(typeToFind);
        }

        public object ValueAs(Type type, string name)
        {
            foreach (var naming in _namingStrategies)
            {
                var actualName = naming(name);
                var rawValue = _request.Value.Value(type, actualName);
                if (rawValue != null)
                {
                    return rawValue;
                }
            }

            return null;
        }

        public bool ValueAs(Type type, string name, Action<object> continuation)
        {
            return _namingStrategies.Any(naming =>
            {
                string n = naming(name);
                return _request.Value.Value(type, n, continuation);
            });
        }

        T IPropertyContext.ValueAs<T>()
        {
            T value = default(T);

            _namingStrategies.Any(naming =>
            {
                string name = naming(Property.Name);
                return _request.Value.Value<T>(name, x => value = x);
            });

            return value;
        }

        bool IPropertyContext.ValueAs<T>(Action<T> continuation)
        {
            return _namingStrategies.Any(naming =>
            {
                string n = naming(Property.Name);
                return _request.Value.Value<T>(n, continuation);
            });
        }

        T IBindingContext.ValueAs<T>(string name)
        {
            T value = default(T);

            _namingStrategies.Any(naming =>
            {
                string n = naming(name);
                return _request.Value.Value<T>(n, x => value = x);
            });

            return value;
        }

        bool IBindingContext.ValueAs<T>(string name, Action<T> continuation)
        {
            return _namingStrategies.Any(naming =>
            {
                var n = naming(name);
                return _request.Value.Value(n, continuation);
            });
        }

        public object PropertyValue { get; protected set; }
        
        private readonly Stack<PropertyInfo> _propertyStack = new Stack<PropertyInfo>();
        public PropertyInfo Property
        {
            get { return _propertyStack.Peek(); }
        }

        private readonly Stack<object> _objectStack = new Stack<object>();



        public void ForProperty(PropertyInfo property, Action<IPropertyContext> action)
        {
            _propertyStack.Push(property);

            try
            {
                findPropertyValueInRequestData(action);
            }
            catch (Exception ex)
            {
                LogProblem(ex);
            }
            finally
            {
                _propertyStack.Pop();
            }
        }

        public void ForObject(object @object, Action action)
        {
            StartObject(@object);
            action();
            FinishObject();
        }

        private void findPropertyValueInRequestData(Action<IPropertyContext> action)
        {
            _namingStrategies.Any(naming =>
            {
                string name = naming(Property.Name);
                return _requestData.Value(name, o =>
                {
                    PropertyValue = o;
                    action(this);
                });
            });
        }

        public object Object
        {
            get { return _objectStack.Any() ? _objectStack.Peek() : null; } 
        }

        public void StartObject(object @object)
        {
            _objectStack.Push(@object);
        }

        public void FinishObject()
        {
            _objectStack.Pop();
        }


        public IBindingContext PrefixWith(string prefix)
        {
            return prefixWith(prefix, _propertyStack.Reverse());
        }

        private BindingContext prefixWith(string prefix, IEnumerable<PropertyInfo> properties)
        {
            var prefixedData = new PrefixedRequestData(_requestData, prefix);
            var child = new BindingContext(prefixedData, _locator, Logger);
            
                
            properties.Each(p => child._propertyStack.Push(p));
            return child;
        }

        public void LogProblem(Exception ex)
        {
            LogProblem(ex.ToString());
        }

        public void LogProblem(string exceptionText)
        {
            var problem = new ConvertProblem()
            {
                ExceptionText = exceptionText,
                Item = Object,
                Properties = _propertyStack.ToArray().Reverse(),
                Value = PropertyValue
            };

            _problems.Add(problem);
        }

        public object BindObject(string prefix, Type childType)
        {
            var resolver = Service<IObjectResolver>();
            var context = prefixWith(prefix, _propertyStack.ToArray().Reverse());

            //need to determine if the item is in there or not since we will be greedy
            //and try to get as many items for our list as possible

            if (_requestData.HasAnyValuePrefixedWith(prefix))
            {
                var bindResult = resolver.BindModel(childType, context);
                return bindResult.Value;
            }
            return null;
        }

        public void BindChild(PropertyInfo property, Type childType, string prefix)
        {
            var target = Object;
            if (_propertyStack.Any(p => p.PropertyType == childType))
            {
                _propertyStack.Push(property);
                throw new FubuException(2202, "Infinite recursion detected while binding child properties: {0} would try to resolve {1} again.", string.Join("=>", _propertyStack.Reverse().Select(p => "{1}.{0}".ToFormat(p.Name, p.ReflectedType.Name)).ToArray()), childType.Name);
            }
            _propertyStack.Push(property);

            var resolver = _locator == null ? ObjectResolver.Basic() : Service<IObjectResolver>();
            var context = prefixWith(prefix, _propertyStack.Reverse());

            try
            {
                resolver.TryBindModel(childType, context, result =>
                {
                    property.SetValue(target, result.Value, null);
                });
                
            }
            catch (Exception e)
            {
                LogProblem(e);
            }

            _problems.AddRange(context._problems);

            _propertyStack.Pop();
        }

        public void BindChild(PropertyInfo property)
        {
            if (!tryBindChild(property, property.Name + "."))
            {
                tryBindChild(property, property.Name);
            }
        }

        private bool tryBindChild(PropertyInfo property, string prefix)
        {
            if (_requestData.HasAnyValuePrefixedWith(prefix))
            {
                BindChild(property, property.PropertyType, prefix);
                return true;
            }

            return false;
        }
    }
}