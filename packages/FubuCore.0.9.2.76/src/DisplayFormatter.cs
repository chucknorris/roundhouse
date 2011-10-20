using System;
using FubuCore.Reflection;
using Microsoft.Practices.ServiceLocation;

namespace FubuCore
{
    public interface IDisplayFormatter
    {
        string GetDisplay(GetStringRequest request);
        string GetDisplay(Accessor accessor, object target);
        string GetDisplayForValue(Accessor accessor, object rawValue);
    }

    public class DisplayFormatter : IDisplayFormatter
    {
        private readonly IServiceLocator _locator;
        private readonly Stringifier _stringifier;

        // IServiceLocator should be injected into the constructor as
        // a dependency
        public DisplayFormatter(IServiceLocator locator, Stringifier stringifier)
        {
            _locator = locator;
            _stringifier = stringifier;
        }

        public string GetDisplay(GetStringRequest request)
        {
            request.Locator = _locator;
            return _stringifier.GetString(request);
        }

        public string GetDisplay(Accessor accessor, object target)
        {
            var request = new GetStringRequest(accessor, target, _locator);
            return _stringifier.GetString(request);
        }

        public string GetDisplayForValue(Accessor accessor, object rawValue)
        {
            var request = new GetStringRequest(accessor, rawValue, _locator);
            return _stringifier.GetString(request);
        }
    }

    public static class DisplayFormatterExtensions
    {
        /// <summary>
        /// Formats the provided value using the accessor accessor metadata and a custom format
        /// </summary>
        /// <param name="formatter">The formatter</param>
        /// <param name="modelType">The type of the model to which the accessor belongs (i.e. Case where the accessor might be on its base class WorkflowItem)</param>
        /// <param name="accessor">The property that holds the given value</param>
        /// <param name="value">The data to format</param>
        /// <param name="format">The custom format specifier</param>
        public static string FormatValue(this IDisplayFormatter formatter, Type modelType, Accessor accessor, object value, string format)
        {
            var request = new GetStringRequest(accessor, value, null)
            {
                Format = format
            };

            return formatter.GetDisplay(request);
        }

        public static string GetDisplayForProperty(this IDisplayFormatter formatter, Accessor accessor, object target)
        {
            return formatter.GetDisplay(accessor, accessor.GetValue(target));
        }

        /// <summary>
        /// Formats the provided value using the property accessor metadata
        /// </summary>
        /// <param name="modelType">The type of the model to which the property belongs (i.e. Case where the property might be on its base class WorkflowItem)</param>
        /// <param name="formatter">The formatter</param>
        /// <param name="property">The property that holds the given value</param>
        /// <param name="value">The data to format</param>
        public static string FormatValue(this IDisplayFormatter formatter, Type modelType, Accessor property, object value)
        {
            return formatter.GetDisplay(new GetStringRequest(property, value, null)
            {
                OwnerType = modelType
            });
        }

        /// <summary>
        /// Retrieves the formatted value of a property from an instance
        /// </summary>
        /// <param name="formatter">The formatter</param>
        /// <param name="modelType">The type of the model to which the property belongs (i.e. Case where the property might be on its base class WorkflowItem)</param>
        /// <param name="property">The property of <paramref name="entity"/> whose value should be formatted</param>
        /// <param name="entity">The instance containing the data to format</param>
        public static string FormatProperty(this IDisplayFormatter formatter, Type modelType, Accessor property, object entity)
        {
            var raw = property.GetValue(entity);
            return formatter.FormatValue(modelType, property, raw);
        }
    }
}