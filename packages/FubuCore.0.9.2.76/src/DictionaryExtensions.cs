using System;
using System.Collections.Generic;

namespace FubuCore
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.Get(key, default(TValue));
        }

        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary.ContainsKey(key)) return dictionary[key];
            return defaultValue;
        }

        /// <summary>
        /// This is a big THANK YOU to the BCL for not hooking a brotha' up
        /// This add will tell WHAT KEY you added twice.
        /// </summary>
        public static void SmartAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            try
            {
                dictionary.Add(key, value);
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException("The key '{0}' already exists.".ToFormat(key), e);
            }
        }
    }
}