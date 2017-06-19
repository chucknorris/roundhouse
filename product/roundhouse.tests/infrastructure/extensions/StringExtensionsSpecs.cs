namespace roundhouse.tests.infrastructure.extensions
{
    using roundhouse.infrastructure.extensions;

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_formats_a_string_using_provided_arguments : TinySpec
    {
        string result;

        public override void Context() {}
        public override void Because() => result = "this is the {0}".format_using(1);

        [Observation]
        public void should_return_the_string_formatted_with_the_arguments()
        {
            result.should_be_equal_to("this is the 1");
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_lower_on_a_regular_string : TinySpec
    {
        static string result;
        private static string test = "BILL1";

        public override void Context() {}
        public override void Because() => result = test.to_lower();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to("bill1");
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_lower_on_an_empty_string : TinySpec
    {
        static string result;
        private static string test = string.Empty;

        public override void Context() {}
        public override void Because() => result = test.to_lower();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to(string.Empty);
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_lower_on_a_null_string : TinySpec
    {
        static string result;
        private static string test = null;

        public override void Context() {}
        public override void Because() => result = test.to_lower();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to(string.Empty);
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_upper_on_a_regular_string : TinySpec
    {
        static string result;
        private static string test = "bill1";


        public override void Context() {}
        public override void Because() => result = test.to_upper();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to("BILL1");
        }
    }


    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_upper_on_an_empty_string : TinySpec
    {
        static string result;

        public override void Context() {}
        public override void Because() => result = string.Empty.to_upper();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to(string.Empty);
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_upper_on_a_null_string : TinySpec
    {
        static string result;
        private static string test = null;

        public override void Context() {}
        public override void Because() => result = test.to_upper();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to(string.Empty);
        }
    }


}