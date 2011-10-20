using System;
using System.Collections.Generic;
using System.Reflection;

namespace FubuCore.Binding
{

    public interface IBindingLogger
    {
        void ChoseModelBinder(Type modelType, IModelBinder binder);
        void ChosePropertyBinder(PropertyInfo property, IPropertyBinder binder);
        void ChoseValueConverter(PropertyInfo property, ValueConverter converter);
    }

    public class NulloBindingLogger : IBindingLogger
    {
        public void ChoseModelBinder(Type modelType, IModelBinder binder)
        {
        }

        public void ChosePropertyBinder(PropertyInfo property, IPropertyBinder binder)
        {
        }

        public void ChoseValueConverter(PropertyInfo property, ValueConverter converter)
        {
        }
    }


    /// <summary>
    /// BindingContext represents the state of model binding as well
    /// as providing some common functionality
    /// </summary>
    public interface IBindingContext
    {

        /// <summary>
        /// Expose logging to the model binding subsystem
        /// </summary>
        IBindingLogger Logger { get; }

        /// <summary>
        /// List of all data conversion errors encountered during
        /// a model binding operation
        /// </summary>
        IList<ConvertProblem> Problems { get; }
        
        /// <summary>
        /// Creates a separate IBindingContext 
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        IBindingContext PrefixWith(string prefix);


        /// <summary>
        /// Performs the binding action in the continuation passed to the method
        /// if a matching value for the property can be found in the request data.
        /// Used primarily from IPropertyBinder's
        /// </summary>
        /// <param name="property"></param>
        /// <param name="action"></param>
        void ForProperty(PropertyInfo property, Action<IPropertyContext> action);

        /// <summary>
        /// CPS method to "tell" the IBindingContext what the current target of
        /// model binding is. 
        /// </summary>
        /// <param name="object"></param>
        /// <param name="action"></param>
        void ForObject(object @object, Action action);

        /// <summary>
        /// Recursively binds request data to a child property of the current
        /// Object using the given childType and prefix 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="childType"></param>
        /// <param name="prefix">Usually just the property name</param>
        void BindChild(PropertyInfo property, Type childType, string prefix);

        /// <summary>
        /// Overload will recursively bind the request data to a child property
        /// of the current object by using the type and name of the PropertyInfo
        /// </summary>
        /// <param name="property"></param>
        void BindChild(PropertyInfo property);

        /// <summary>
        /// Creates an object of the childType bound to the request data
        /// using the prefix given
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="childType"></param>
        /// <returns></returns>
        object BindObject(string prefix, Type childType);

        /// <summary>
        /// The current object being bound
        /// </summary>
        object Object { get; }

        /// <summary>
        /// Service locator method to the IoC container for the current request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Service<T>();

        /// <summary>
        /// Fetch the value from the current request by name and convert the raw value
        /// to the supplied type.  Respects the current prefix.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        object ValueAs(Type type, string name);

        /// <summary>
        /// CPS style call to ValueAs().  The continuation is only called if the named value is in the 
        /// current request data.  Respects the current prefix
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        bool ValueAs(Type type, string name, Action<object> continuation);

        /// <summary>
        /// Fetches the value in the request data by name and converts the value
        /// to the supplied type.  Respects the current prefix.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        T ValueAs<T>(string name);

        /// <summary>
        /// CPS style call to ValueAs<T>().  The continuation is only called if the named value is
        /// in the current request data.  Respects the current prefix.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="continuation"></param>
        /// <returns></returns>
        bool ValueAs<T>(string name, Action<T> continuation);
    }

    /// <summary>
    /// Represents the current property in a model binding
    /// session.  This interface is typically consumed within
    /// custom PropertyBinder's
    /// </summary>
    public interface IPropertyContext
    {
        /// <summary>
        /// The raw value in the current request data
        /// that matches the name of the current property.
        /// </summary>
        object PropertyValue { get; }

        /// <summary>
        /// The current property
        /// </summary>
        PropertyInfo Property { get; }

        /// <summary>
        /// The current object being bound
        /// </summary>
        object Object { get; }

        /// <summary>
        /// Service locator method to the IoC container for the current request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Service<T>();

        /// <summary>
        /// Service locator method to the IoC container for the current request
        /// </summary>
        /// <param name="typeToFind">The type to find</param>
        /// <returns></returns>
        object Service(Type typeToFind);

        /// <summary>
        /// Retrieves the PropertyValue converted to the type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ValueAs<T>();

        /// <summary>
        /// CPS version of ValueAs.  The continuation is only called if there is
        /// data in the current request data with the same name as the property
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="continuation"></param>
        /// <returns></returns>
        bool ValueAs<T>(Action<T> continuation);

        /// <summary>
        /// Expose logging to the model binding subsystem
        /// </summary>
        IBindingLogger Logger { get; }
    }
}