namespace roundhouse.infrastructure.extensions
{
    public static class StringExtensions
    {
        public static string format_using(this string format, params object[] args)
        {
            return string.Format(format, args);
        }

        public static string to_lower(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return input.ToLower();
        }

        public static string to_upper(this string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            return input.ToUpper();
        }
    }
}