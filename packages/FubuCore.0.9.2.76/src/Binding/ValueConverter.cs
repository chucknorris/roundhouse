using System;

namespace FubuCore.Binding
{
    public interface ValueConverter
    {
        object Convert(IPropertyContext context);
    }

    public class LambdaValueConverter : ValueConverter
    {
        private readonly Func<IPropertyContext, object> _converter;

        public LambdaValueConverter(Func<IPropertyContext, object> converter)
        {
            _converter = converter;
        }

        public object Convert(IPropertyContext context)
        {
            return _converter(context);
        }
    }
}