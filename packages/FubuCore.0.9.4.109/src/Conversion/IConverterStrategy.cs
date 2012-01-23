namespace FubuCore.Conversion
{
    public interface IConverterStrategy
    {
        object Convert(IConversionRequest request);
    }
}