namespace FubuCore.Conversion
{
    public static class ConverterStrategyExtensions
    {
        public static object Convert(this IConverterStrategy strategy, string text)
        {
            return strategy.Convert(new ConversionRequest(text));
        }
    }
}