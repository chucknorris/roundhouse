using System;

namespace FubuCore.Conversion
{
    public class EnumConverterFamily : IObjectConverterFamily
    {
        public bool Matches(Type type, ConverterLibrary converter)
        {
            return type.IsEnum;
        }

        public IConverterStrategy CreateConverter(Type type, Func<Type, IConverterStrategy> converterSource)
        {
            return new EnumConversionStrategy(type);
        }

        #region Nested type: EnumConversionStrategy

        public class EnumConversionStrategy : IConverterStrategy
        {
            private readonly Type _enumType;

            public EnumConversionStrategy(Type enumType)
            {
                _enumType = enumType;
            }

            public object Convert(IConversionRequest request)
            {
                return Enum.Parse(_enumType, request.Text, true);
            }
        }

        #endregion
    }
}