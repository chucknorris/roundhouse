namespace roundhouse.tests.infrastructure.extensions
{
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.infrastructure.extensions;

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_formats_a_string_using_provided_arguments : observations_for_a_static_sut
    {
        static string result;

        because b = () => result = "this is the {0}".format_using(1);

        [Observation]
        public void should_return_the_string_formatted_with_the_arguments()
        {
            result.should_be_equal_to("this is the 1");
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_lower_on_a_regular_string : observations_for_a_static_sut
    {
        static string result;
        private static string test = "BILL1";


        private because b = () => result = test.to_lower();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to("bill1");
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_lower_on_an_empty_string : observations_for_a_static_sut
    {
        static string result;
        private static string test = string.Empty;

        private because b = () => result = test.to_lower();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to(string.Empty);
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_lower_on_a_null_string : observations_for_a_static_sut
    {
        static string result;
        private static string test = null;

        private because b = () => result = test.to_lower();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to(string.Empty);
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_upper_on_a_regular_string : observations_for_a_static_sut
    {
        static string result;
        private static string test = "bill1";


        private because b = () => result = test.to_upper();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to("BILL1");
        }
    }


    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_upper_on_an_empty_string : observations_for_a_static_sut
    {
        static string result;

        private because b = () => result = string.Empty.to_upper();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to(string.Empty);
        }
    }

    [Concern(typeof(StringExtensions))]
    public class when_the_string_extensions_attempts_to_upper_on_a_null_string : observations_for_a_static_sut
    {
        static string result;
        private static string test = null;

        private because b = () => result = test.to_upper();

        [Observation]
        public void should_not_error_out()
        {
            result.should_be_equal_to(string.Empty);
        }
    }


}