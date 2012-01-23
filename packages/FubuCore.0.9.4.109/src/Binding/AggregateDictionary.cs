using System;
using System.Collections.Generic;
using System.Linq;

namespace FubuCore.Binding
{
    public class AggregateDictionary
    {
        private readonly IList<Locator> _locators = new List<Locator>();

        public bool HasAnyValuePrefixedWith(string key)
        {
            return _locators.Any(x => x.StartsWith(key));
        }

        public void Value(string key, Action<string, object> callback)
        {
            _locators.Any(x => x.Locate(key, callback));
        }

        public void Value(string source, string key, Action<string, object> callback)
        {
            var locator = findLocator(source);
            locator.Locate(key, callback);
        }

        private Locator findLocator(string source)
        {
            return _locators.First(x => x.Source == source);
        }

        public AggregateDictionary AddLocator(string source, Func<string, object> locator,
                                              Func<IEnumerable<string>> allKeys)
        {
            _locators.Add(new Locator{
                Getter = locator,
                Source = source,
                AllKeys = allKeys
            });

            return this;
        }

        public AggregateDictionary AddDictionary(string source, IDictionary<string, object> dictionary)
        {
            AddLocator(source, key => dictionary.ContainsKey(key) ? dictionary[key] : null,
                       () => dictionary.Keys);
            return this;
        }

        public IEnumerable<string> GetAllKeys()
        {
            return _locators.SelectMany(locator => locator.AllKeys()).Distinct();
        }

        public IRequestData DataFor(string source)
        {
            var dictionary = new AggregateDictionary();
            var locator = findLocator(source);
            dictionary._locators.Add(locator);

            return new RequestData(dictionary);
        }
    }

    public class Locator
    {
        public Func<IEnumerable<string>> AllKeys { get; set; }
        public string Source { get; set; }

        public Func<string, object> Getter { get; set; }

        public bool Locate(string key, Action<string, object> callback)
        {
            var value = Getter(key);
            if (value != null)
            {
                callback(Source, value);
                return true;
            }

            return false;
        }

        public bool StartsWith(string key)
        {
            return AllKeys().Any(x => x.StartsWith(key));
        }
    }
}