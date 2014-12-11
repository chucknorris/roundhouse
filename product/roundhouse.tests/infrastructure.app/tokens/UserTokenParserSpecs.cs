using System;
using System.Collections.Generic;
using bdddoc.core;
using developwithpassion.bdd.contexts;
using developwithpassion.bdd.mbunit;
using developwithpassion.bdd.mbunit.standard;

namespace roundhouse.tests.infrastructure.app.tokens
{
    using roundhouse.infrastructure.app.tokens;
    using System.IO;

    public class UserTokenParserSpecs
    {
        [Concern(typeof(UserTokenParser))]
        public class when_parsing_from_text
        {
            protected static object result;

            context c = () =>
            { };

            [Observation]
            public void if_given_keyvalues_should_parse_to_dictionary()
            {
                var dictionary = UserTokenParser.Parse("UserId=123&UserName=Some Name");
                dictionary.should_not_be_an_instance_of<Dictionary<string, string>>();
                dictionary.should_only_contain(
                    new KeyValuePair<string, string>("UserId", "123"),
                    new KeyValuePair<string, string>("UserName", "Some Name"));
            }
            [Observation]
            public void if_given_filepath_with_keyvalues_should_parse_to_dictionary()
            {
                var filename = Guid.NewGuid().ToString("N") + ".txt";
                File.WriteAllText(filename, "UserId=123&UserName=Some Name");
                try
                {
                    var dictionary = UserTokenParser.Parse(filename);
                    dictionary.should_not_be_an_instance_of<Dictionary<string, string>>();
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