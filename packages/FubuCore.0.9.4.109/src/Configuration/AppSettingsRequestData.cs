using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using FubuCore.Binding;
using System.Linq;
using FubuCore.Reflection;

namespace FubuCore.Configuration
{
    public class AppSettingsRequestData : IRequestData
    {
        public object Value(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public bool Value(string key, Action<object> callback)
        {
            if (ConfigurationManager.AppSettings.AllKeys.Contains(key))
            {
                callback(Value(key));
                return true;
            }

            return false;
        }

        public bool HasAnyValuePrefixedWith(string key)
        {
            return ConfigurationManager.AppSettings.AllKeys.Any(x => x.StartsWith(key));
        }

        public static string KeyFor<T>(Expression<Func<T, object>> property)
        {
            return typeof(T).Name + "." + property.ToAccessor().Name;
        }

        public static string GetValueFor<T>(Expression<Func<T, object>> property)
        {
            var key = KeyFor(property);
            return (ConfigurationManager.AppSettings.AllKeys.Contains(key)) ? ConfigurationManager.AppSettings[key] : string.Empty;
        }

        public IEnumerable<string> GetKeys()
        {
            return ConfigurationManager.AppSettings.AllKeys;
        }
    }
}