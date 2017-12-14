using System;
using System.Collections.Generic;
using roundhouse.infrastructure.app;

namespace roundhouse.tests.infrastructure.app.tokens
{
    using roundhouse.infrastructure.app.tokens;
    using System.IO;

    public class UserTokenParserSpecs
    {
        [Concern(typeof(UserTokenParser))]
        public class when_parsing_from_text : TinySpec
        {
            protected static object result;

            public override void Context()
            {
            }

            public override void Because()
            {
            }

            [Observation]
            public void empty_entry_should_be_ignored()
            {
                var dictionary = UserTokenParser.Parse("=");
                dictionary.should_be_empty();
            }
            [Observation]
            public void empty_key_should_be_ignored()
            {
                var dictionary = UserTokenParser.Parse("=42");
                dictionary.should_be_empty();
            }
            [Observation]
            public void empty_value_should_parse_to_empty_option()
            {
                var dictionary = UserTokenParser.Parse("Bob=");
                dictionary.should_be_an_instance_of<Dictionary<string, string>>();
                dictionary.should_only_contain(
                    new KeyValuePair<string, string>("Bob",string.Empty)
                );
            }
            [Observation]
            public void if_given_keyvalues_should_parse_to_dictionary()
            {
                var dictionary = UserTokenParser.Parse("UserId=123;UserName=Some Name");
                dictionary.should_be_an_instance_of<Dictionary<string, string>>();
                dictionary.should_only_contain(
                    new KeyValuePair<string, string>("UserId", "123"),
                    new KeyValuePair<string, string>("UserName", "Some Name"));
            }

            [Observation]
            public void if_given_filepath_with_keyvalues_should_parse_to_dictionary()
            {
                var filename = Path.GetTempFileName() + ".txt";
                File.WriteAllText(filename, "UserId=123" + Environment.NewLine + "UserName=Some Name");
                try
                {
                    var dictionary = UserTokenParser.Parse(filename);
                    dictionary.should_be_an_instance_of<Dictionary<string, string>>();
                    dictionary.should_only_contain(
                        new KeyValuePair<string, string>("UserId", "123"),
                        new KeyValuePair<string, string>("UserName", "Some Name"));
                }
                finally
                {
                    File.Delete(filename);
                }
            }
            [Observation]
            public void if_given_wrong_syntax_text_without_equals_sign_should_throw_format_exception()
            {
                Action action = () =>
                {
                    var dictionary = UserTokenParser.Parse("UserId123User&NameSome Name");
                };
                action.should_throw_an<FormatException>();
            }
            [Observation]
            public void if_given_empty_text_should_throw_argument_null_exception()
            {
                Action action = () =>
                {
                    var dictionary = UserTokenParser.Parse("");
                };
                action.should_throw_an<ArgumentNullException>();
            }
        }

    }
}