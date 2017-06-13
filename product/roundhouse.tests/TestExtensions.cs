using Xunit;

namespace roundhouse.tests
{
    public static class TestExtensions
    {
        public static void should_not_contain(this string item, string test_value)
        {
            Assert.DoesNotContain(test_value, item);
        }

        public static void should_contain(this string item, string test_value)
        {
            Assert.Contains(test_value, item);
        }
    }
}