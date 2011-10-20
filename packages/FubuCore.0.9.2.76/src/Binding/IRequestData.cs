using System;
using System.Collections.Generic;

namespace FubuCore.Binding
{
    public interface IRequestData
    {
        object Value(string key);
        bool Value(string key, Action<object> callback);
        bool HasAnyValuePrefixedWith(string key);
        IEnumerable<string> GetKeys();
    }
}