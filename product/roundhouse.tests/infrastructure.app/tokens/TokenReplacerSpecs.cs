using bdddoc.core;
using developwithpassion.bdd.contexts;
using developwithpassion.bdd.mbunit;
using developwithpassion.bdd.mbunit.standard;
using developwithpassion.bdd.mbunit.standard.observations;

namespace roundhouse.tests.infrastructure.app.tokens
{
    using System;
    using consoles;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.app.tokens;

    public class TokenReplacerSpecs
    {
        public abstract class concern_for_TokenReplacer : observations_for_a_static_sut
        {
            protected static object result;
            protected static ConfigurationPropertyHolder configuration;
            protected static string database_name = "BOB";

            context c = () =>
            {
                configuration = new ConsoleConfiguration { DatabaseName = database_name };
            };
        }

        [Concern(typeof(TokenReplacer))]
        public class when_replacing_tokens_in_sql_files_using_the_configuration : concern_for_TokenReplacer
        {

            because b = () => { };

            [Observation]
            public void if_given_bracket_bracket_DatabaseName_bracket_bracket_should_replace_with_the_DatabaseName_from_the_configuration()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{DatabaseName}}").should_be_equal_to("ALTER DATABASE " + database_name);
            }

            [Observation]
            public void if_given_bracket_DatabaseName_bracket_should_NOT_replace_the_value()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {DatabaseName}").should_be_equal_to("ALTER DATABASE {DatabaseName}");
            }

            [Observation]
            public void if_given_a_value_that_is_the_name_of_a_configuration_item_but_is_not_properly_tokenized_it_should_NOT_replace_the_value()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE DatabaseName").should_be_equal_to("ALTER DATABASE DatabaseName");
            }

            [Observation]
            public void if_given_bracket_bracket_databasename_bracket_bracket_should_replace_with_the_DatabaseName_from_the_configuration()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{databasename}}").should_be_equal_to("ALTER DATABASE " + database_name);
            }

            [Observation]
            public void if_given_bracket_bracket_DATABASENAME_bracket_bracket_should_replace_with_the_DatabaseName_from_the_configuration()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{DATABASENAME}}").should_be_equal_to("ALTER DATABASE " + database_name);
            }

            [Observation]
            public void if_given_bracket_bracket_ServerName_bracket_bracket_should_NOT_replace_with_the_DatabaseName_from_the_configuration()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{servername}}").should_not_contain(database_name);
            }

            [Observation]
            public void if_given_a_value_that_is_not_set_should_return_empty_string()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{servername}}").should_be_equal_to("ALTER DATABASE " + string.Empty);
            }

            [Observation]
            public void if_given_a_value_that_does_not_exist_should_error()
            {
                bool error_happened = false;
                try
                {
                    TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{database}}");
                }
                catch (Exception)
                {
                    error_happened = true;
                }

                error_happened.should_be_true();

            }
        }
    }
}