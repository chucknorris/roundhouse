using FluentAssertions;
using roundhouse.cryptography;
using roundhouse.databases;
using roundhouse.databases.sqlserver;
using Xunit;

namespace roundhouse.tests.infrastructure.containers
{
    using consoles;
    using environments;
    using migrators;

    public class DefaultDatabaseMigratorSpecs
    {
        public abstract class concern_for_database_migrator 
        {
            protected object result;
            protected DefaultEnvironment environment;
            protected DefaultDatabaseMigrator sut;

            protected concern_for_database_migrator()
            {
                sut = new DefaultDatabaseMigrator(new MockDatabase(new SqlServerDatabase()), new MD5CryptographicService(), new DefaultConfiguration());
                environment = new DefaultEnvironment(new DefaultConfiguration { EnvironmentName = "TEST" });
            }
        }

        public class when_determining_if_we_are_in_the_right_environment : concern_for_database_migrator
        {
            [Fact]
            public void if_given_TEST_at_the_front_and_in_TEST_should_return_true()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("TEST.something.ENV.sql", environment).Should().BeTrue();
            }

            [Fact]
            public void if_given_TEST_in_the_middle_and_in_TEST_should_return_true()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("something.TEST.ENV.sql", environment).Should().BeTrue();
            }

            [Fact]
            public void if_given_PROD_and_in_TEST_should_return_false()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("PROD.something.ENV.sql", environment).Should().BeFalse();
            }

            [Fact]
            public void if_given_BOBTEST_at_the_front_and_in_TEST_should_return_false()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("BOBTEST.something.ENV.sql", environment).Should().BeFalse();
            }

            [Fact]
            public void if_given_BOBTEST_in_the_middle_and_in_TEST_should_return_false()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("something.BOBTEST.ENV.sql", environment).Should().BeFalse();
            }
        }

        public class when_determining_if_we_are_running_an_everytime_script : concern_for_database_migrator
        {
            [Fact]
            public void if_given_EVERYTIME_at_the_front_it_should_return_true()
            {
                sut.this_is_an_every_time_script("EVERYTIME.something.sql", false).Should().BeTrue();
            }

            [Fact]
            public void if_given_EVERYTIME_in_the_middle_it_should_return_true()
            {
                sut.this_is_an_every_time_script("something.EVERYTIME.sql", false).Should().BeTrue();
            }

            [Fact]
            public void if_passed_true_for_run_everytime_it_should_return_true_no_matter_what_the_name_of_the_script_is()
            {
                sut.this_is_an_every_time_script("something.sql", true).Should().BeTrue();
            }

            [Fact]
            public void if_given_TIME_it_should_return_false()
            {
                sut.this_is_an_every_time_script("something.TIME.sql", false).Should().BeFalse();
            }

        }


    }
}