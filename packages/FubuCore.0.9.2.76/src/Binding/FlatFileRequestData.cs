using System;
using System.Collections.Generic;
using FubuCore.Util;

namespace FubuCore.Binding
{
    public class FlatFileRequestData : IRequestData
    {
        private readonly string _concatenator;
        private readonly Cache<string, int> _indices = new Cache<string, int>();
        private string[] _values;

        public FlatFileRequestData(string concatenator, string headerLine)
        {
            _concatenator = concatenator;
            var headers = headerLine.Split(new string[] { _concatenator }, StringSplitOptions.None);
            for (int i = 0; i < headers.Length; i++)
            {
                _indices[headers[i]] = i;
            }
        }

        public void ReadLine(string line)
        {
            _values = line.Split(new string[] { _concatenator }, StringSplitOptions.None);
        }

        public object Value(string key)
        {
            return _values[_indices[key]];
        }

        public bool Value(string key, Action<object> callback)
        {
            bool found = false;
            _indices.WithValue(key, index =>
            {
                callback(_values[index]);
                found = true;
            });

            return found;
        }

        public bool HasAnyValuePrefixedWith(string key)
        {
            throw new NotSupportedException();
        }

        public void Alias(string header, string alias)
        {
            _indices.WithValue(header, i => _indices[alias] = i);
        }

        public IEnumerable<string> GetKeys()
        {
            return _indices.GetAllKeys();
        }
    }
}