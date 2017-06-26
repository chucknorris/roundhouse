using FluentAssertions;
using Xunit;

namespace roundhouse.tests.infrastructure.extensions
{
    using roundhouse.infrastructure.extensions;

    public class when_the_string_extensions_formats_a_string_using_provided_arguments
    {
        private readonly string result;

        public when_the_string_extensions_formats_a_string_using_provided_arguments()
        {
            result = "this is the {0}".format_using(1);
        }

        [Fact]
        public void should_return_the_string_formatted_with_the_arguments()
        {
            result.Should().Be("this is the 1");
        }
    }

    public class when_the_string_extensions_attempts_to_lower_on_a_regular_string
    {
        string result;
        private string test = "BILL1";

        public when_the_string_extensions_attempts_to_lower_on_a_regular_string()
        {
            result = test.to_lower();
        }

        [Fact]
        public void should_not_error_out()
        {
            result.Should().Be("bill1");
        }
    }

    public class when_the_string_extensions_attempts_to_lower_on_an_empty_string
    {
        string result;
        private readonly string test = string.Empty;

        public when_the_string_extensions_attempts_to_lower_on_an_empty_string()
        {
            result = test.to_lower();
        }

        [Fact]
        public void should_not_error_out()
        {
            result.Should().Be(string.Empty);
        }
    }

    public class when_the_string_extensions_attempts_to_lower_on_a_null_string
    {
        string result;
        private readonly string test = null;

        public when_the_string_extensions_attempts_to_lower_on_a_null_string()
        {
            result = test.to_lower();
        }

        [Fact]
        public void should_not_error_out()
        {
            result.Should().Be(string.Empty);
        }
    }

    public class when_the_string_extensions_attempts_to_upper_on_a_regular_string
    {
        readonly string result;
        private string test = "bill1";

        public when_the_string_extensions_attempts_to_upper_on_a_regular_string()
        {
            result = test.to_upper();
        }

        [Fact]
        public void should_not_error_out()
        {
            result.Should().Be("BILL1");
        }
    }


    public class when_the_string_extensions_attempts_to_upper_on_an_empty_string 
    {
        readonly string result;

        public when_the_string_extensions_attempts_to_upper_on_an_empty_string()
        {
            result = string.Empty.to_upper(); 
        }

        [Fact]
        public void should_not_error_out()
        {
            result.Should().Be(string.Empty);
        }
    }

    public class when_the_string_extensions_attempts_to_upper_on_a_null_string 
    {
        string result;
        private readonly string test = null;

        public when_the_string_extensions_attempts_to_upper_on_a_null_string()
        {
            result = test.to_upper(); ;
        }

        [Fact]
        public void should_not_error_out()
        {
            result.Should().Be(string.Empty);
        }
    }


}