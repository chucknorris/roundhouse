using System;

namespace FubuCore.Conversion
{
    // TODO -- come back here and add the object descriptor stuff
    // for diagnostics
    public class LambdaConverterStrategy<T> : IConverterStrategy
    {
        private readonly Func<string, T> _finder;

        public LambdaConverterStrategy(Func<string, T> finder)
        {
            _finder = finder;
        }

        public object Convert(IConversionRequest request)
        {
            return _finder(request.Text);
        }
    }


    // TODO -- come back here and add the object descriptor stuff
    // for diagnostics
    public class LambdaConverterStrategy<TReturnType, TService> : IConverterStrategy
    {
        private readonly Func<TService, string, TReturnType> _finder;

        public LambdaConverterStrategy(Func<TService, string, TReturnType> finder)
        {
            _finder = finder;
        }

        public object Convert(IConversionRequest request)
        {
            return _finder(request.Get<TService>(), request.Text);
        }
    }
}