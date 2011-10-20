namespace Bottles
{
    public static class StringExtensions
    {
        public static string ToSubstitution(this string value)
        {
            return "{" + value + "}";
        }
    }
}