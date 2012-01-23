using System;

namespace FubuCore.Conversion
{
    public class NullableConverterFamily : IObjectConverterFamily
    {
        public bool Matches(Type type, ConverterLibrary converter)
        {
            return type.IsNullable();
        }

        public IConverterStrategy CreateConverter(Type type, Func<Type, IConverterStrategy> converterSource)
        {
            var inner = converterSource(type.GetInnerTypeFromNullable());
            return new NullableConverterStrategy(inner, type);
        }
    }

    public class NullableConverterStrategy : IConverterStrategy
    {
        private readonly IConverterStrategy _inner;
        private readonly Type _type;

        public NullableConverterStrategy(IConverterStrategy inner, Type type)
        {
            _inner = inner;
            _type = type;
        }

        public object Convert(IConversionRequest request)
        {
            var stringValue = request.Text;
            if (stringValue == ObjectConverter.NULL || stringValue == null) return null;
            if (stringValue == string.Empty && _type != typeof (string)) return null;

            return _inner.Convert(request);
        }
    }
}