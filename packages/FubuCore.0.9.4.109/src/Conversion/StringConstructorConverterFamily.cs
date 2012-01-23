using System;
using FubuCore.Reflection.Expressions;

namespace FubuCore.Conversion
{
    public class StringConstructorConverterFamily : IObjectConverterFamily
    {
        public bool Matches(Type type, ConverterLibrary converter)
        {
            if (type.IsArray) return false;

            var constructorInfo = type.GetConstructor(new[]{typeof (string)});
            return constructorInfo != null;
        }

        public IConverterStrategy CreateConverter(Type type, Func<Type, IConverterStrategy> converterSource)
        {
            return typeof (FuncBuilder<>).CloseAndBuildAs<IConverterStrategy>(type);
        }

        #region Nested type: FuncBuilder

        public class FuncBuilder<T> : IConverterStrategy
        {
            private readonly Func<string, T> _func;

            public FuncBuilder()
            {
                _func = ConstructorBuilder.CreateSingleStringArgumentConstructor(typeof (T))
                    .Compile().As<Func<string, T>>();
            }

            public object Convert(IConversionRequest request)
            {
                return _func(request.Text);
            }
        }

        #endregion
    }
}