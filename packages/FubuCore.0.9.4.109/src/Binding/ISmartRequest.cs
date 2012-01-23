using System;

namespace FubuCore.Binding
{
    public interface ISmartRequest
    {
        object Value(Type type, string key);
        bool Value(Type type, string key, Action<object> continuation);

        T Value<T>(string key);
        bool Value<T>(string key, Action<T> callback);


    }

    public class InMemorySmartRequest : ISmartRequest
    {
        private readonly InMemoryRequestData _request = new InMemoryRequestData();

        public object this[string key]
        {
            get
            {
                return _request[key];
            }
            set
            {
                _request[key] = value;
            }
        }

        public object Value(Type type, string key)
        {
            return _request.Value(key);
        }

        public bool Value(Type type, string key, Action<object> continuation)
        {
            return _request.Value(key, continuation);
        }

        public T Value<T>(string key)
        {
            return (T) _request.Value(key);
        }

        public bool Value<T>(string key, Action<T> callback)
        {
            return _request.Value(key, o => callback((T) o));
        }

    }
}