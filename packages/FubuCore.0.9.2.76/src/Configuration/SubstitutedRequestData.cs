using System;
using System.Collections.Generic;
using FubuCore.Binding;

namespace FubuCore.Configuration
{
    public class SubstitutedRequestData : IRequestData
    {
        private readonly IRequestData _inner;
        private readonly IKeyValues _substitutions;

        public SubstitutedRequestData(IRequestData inner, IKeyValues substitutions)
        {
            _inner = inner;
            _substitutions = substitutions;
        }

        public object Value(string key)
        {
            var rawValue = _inner.Value(key);
            if (rawValue == null) return null;

            var parsedValue = rawValue.ToString();

            parsedValue = TemplateParser.Parse(parsedValue, _substitutions);

            return parsedValue;
        }

        public bool Value(string key, Action<object> callback)
        {
            return _inner.Value(key, o =>
            {
                var substitutedValue = TemplateParser.Parse(o.ToString(), _substitutions);
                callback(substitutedValue);
            });
        }

        public bool HasAnyValuePrefixedWith(string key)
        {
            return _inner.HasAnyValuePrefixedWith(key);
        }

        public IEnumerable<string> GetKeys()
        {
            return _inner.GetKeys();
        }
    }
}