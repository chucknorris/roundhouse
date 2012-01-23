namespace FubuCore.Conversion
{
    public class StringConverterStrategy : StatelessConverter<string>
    {
        public const string EMPTY = "EMPTY";
        public const string BLANK = "BLANK";

        protected override string convert(string text)
        {
            var stringValue = text;
            if (stringValue == BLANK || stringValue == EMPTY)
            {
                return string.Empty;
            }

            if (stringValue == ObjectConverter.NULL)
            {
                return null;
            }

            return stringValue;
        }
    }
}