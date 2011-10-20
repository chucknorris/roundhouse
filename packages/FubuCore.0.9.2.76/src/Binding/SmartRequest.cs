using System;

namespace FubuCore.Binding
{
    public class SmartRequest : ISmartRequest
    {
        private readonly IRequestData _data;
        private readonly IObjectConverter _converter;

        public SmartRequest(IRequestData data, IObjectConverter converter)
        {
            _data = data;
            _converter = converter;
        }

        public virtual object Value(Type type, string key)
        {
            object returnValue = null;

            if (_converter.CanBeParsed(type))
            {
                _data.Value(key, o =>
                {
                    returnValue = convertValue(o, type);
                });
            }

            return returnValue;
        }

        public bool Value(Type type, string key, Action<object> continuation)
        {
            if (_converter.CanBeParsed(type))
            {
                return _data.Value(key, o =>
                {
                    var value = convertValue(o, type);
                    continuation(value);
                });
            }

            return false;
        }

        private object convertValue(object rawValue, Type type)
        {
            if (rawValue == null) return null;

            if (rawValue.GetType().CanBeCastTo(type)) return rawValue;

            return _converter.FromString(rawValue.ToString(), type);
        }

        public T Value<T>(string key)
        {
            return (T) Value(typeof (T), key);
        }

        public bool Value<T>(string key, Action<T> callback)
        {
            return _data.Value(key, raw =>
            {
                var value = (T)convertValue(raw, typeof (T));
                callback(value);
            });
        }

        public virtual ISmartRequest PrefixedWith(string prefix)
        {
            return new SmartRequest(new PrefixedRequestData(_data, prefix), _converter);
        }
    }
}