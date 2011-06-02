namespace roundhouse.tests
{
    using MbUnit.Framework;

    public static class TestExtensions
    {
        public static void should_not_contain(this string item, string test_value)
        {
            Assert.IsFalse(item.Contains(test_value));
        }
    }
}