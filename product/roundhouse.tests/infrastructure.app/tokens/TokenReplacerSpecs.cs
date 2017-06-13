using FluentAssertions;
using Xunit;

namespace roundhouse.tests.infrastructure.app.tokens
{
    using consoles;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.app.tokens;

    public class TokenReplacerSpecs
    {
        public abstract class concern_for_TokenReplacer 
        {
            protected object result;
            protected ConfigurationPropertyHolder configuration;
            protected static string database_name = "BOB";

            protected concern_for_TokenReplacer()
            {
                configuration = new DefaultConfiguration { DatabaseName = database_name };
            }
        }

        public class when_replacing_tokens_in_sql_files_using_the_configuration : concern_for_TokenReplacer
        {

            [Fact]
            public void if_given_bracket_bracket_DatabaseName_bracket_bracket_should_replace_with_the_DatabaseName_from_the_configuration()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{DatabaseName}}").Should().Be("ALTER DATABASE " + database_name);
            }

            [Fact]
            public void if_given_bracket_DatabaseName_bracket_should_NOT_replace_the_value()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {DatabaseName}").Should().Be("ALTER DATABASE {DatabaseName}");
            }

            [Fact]
            public void if_given_a_value_that_is_the_name_of_a_configuration_item_but_is_not_properly_tokenized_it_should_NOT_replace_the_value()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE DatabaseName").Should().Be("ALTER DATABASE DatabaseName");
            }

            [Fact]
            public void if_given_bracket_bracket_databasename_bracket_bracket_should_replace_with_the_DatabaseName_from_the_configuration()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{databasename}}").Should().Be("ALTER DATABASE " + database_name);
            }

            [Fact]
            public void if_given_bracket_bracket_DATABASENAME_bracket_bracket_should_replace_with_the_DatabaseName_from_the_configuration()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{DATABASENAME}}").Should().Be("ALTER DATABASE " + database_name);
            }

            [Fact]
            public void if_given_bracket_bracket_ServerName_bracket_bracket_should_NOT_replace_with_the_DatabaseName_from_the_configuration()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{servername}}").should_not_contain(database_name);
            }

            [Fact]
            public void if_given_a_value_that_is_not_set_should_return_empty_string()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{servername}}").Should().Be("ALTER DATABASE " + string.Empty);
            }

            [Fact]
            public void if_given_a_value_that_does_not_exist_should_return_the_value_with_original_casing()
            {
                TokenReplacer.replace_tokens(configuration, "ALTER DATABASE {{DataBase}}").Should().Be("ALTER DATABASE {{DataBase}}");
            }
        }
    }
}