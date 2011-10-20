using System;
using System.Collections.Generic;

namespace FubuCore.Binding
{
    public class PrefixedRequestData : IRequestData
    {
        private readonly IRequestData _inner;
        private readonly string _prefix;

        public PrefixedRequestData(IRequestData inner, string prefix)
        {
            _inner = inner;
            _prefix = prefix;
        }

        public object Value(string key)
        {
            return _inner.Value(_prefix + key);
        }

        public bool Value(string key, Action<object> callback)
        {
            return _inner.Value(_prefix + key, callback);
        }

        public bool HasAnyValuePrefixedWith(string key)
        {
            return _inner.HasAnyValuePrefixedWith(_prefix + key);
        }

        public IEnumerable<string> GetKeys()
        {
            return _inner.GetKeys();
        }
    }
}