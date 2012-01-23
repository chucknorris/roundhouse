using System;

namespace FubuCore.Conversion
{
    public abstract class StatelessConverter : IObjectConverterFamily, IConverterStrategy
    {
        public abstract object Convert(IConversionRequest request);
        public abstract bool Matches(Type type, ConverterLibrary converter);

        public IConverterStrategy CreateConverter(Type type, Func<Type, IConverterStrategy> converterSource)
        {
            return this;
        }
    }

    public abstract class StatelessConverter<TReturnType> : StatelessConverter
    {
        public override sealed bool Matches(Type type, ConverterLibrary converter)
        {
            return type == typeof (TReturnType);
        }

        public override sealed object Convert(IConversionRequest request)
        {
            return convert(request.Text);
        }

        protected abstract TReturnType convert(string text);
    }

    public abstract class StatelessConverter<TReturnType, TService> : StatelessConverter
    {
        public override sealed bool Matches(Type type, ConverterLibrary converter)
        {
            return type == typeof(TReturnType);
        }

        public override sealed object Convert(IConversionRequest request)
        {
            var service = request.Get<TService>();
            return convert(service, request.Text);
        }

        protected abstract TReturnType convert(TService service, string text);
    }
}