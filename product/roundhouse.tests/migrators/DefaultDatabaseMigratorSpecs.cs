using roundhouse.databases;
using roundhouse.databases.sqlserver;
using Should;

namespace roundhouse.tests.infrastructure.containers
{
    using System;
    using consoles;

    using environments;
    using migrators;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.containers;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;
    using StructureMap;
    using Container = roundhouse.infrastructure.containers.Container;

    public class DefaultDatabaseMigratorSpecs
    {
        public abstract class concern_for_database_migrator : TinySpec<DefaultDatabaseMigrator>
        {
            protected static object result;
            protected static DefaultEnvironment environment;
            private DefaultConfiguration default_configuration;

            public override void Context()
            {
                default_configuration = new DefaultConfiguration { EnvironmentName = "TEST" };
                environment = new DefaultEnvironment(default_configuration);
            }

            protected override DefaultDatabaseMigrator sut => new DefaultDatabaseMigrator(new MockDatabase(null), null, default_configuration);
        }

        [Concern(typeof(DefaultDatabaseMigrator))]
        public class when_determining_if_we_are_in_the_right_environment : concern_for_database_migrator
        {
            public override void Because() { }

            [Observation]
            public void if_given_TEST_at_the_front_and_in_TEST_should_return_true()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("TEST.something.ENV.sql", environment).ShouldBeTrue();
            }

            [Observation]
            public void if_given_TEST_in_the_middle_and_in_TEST_should_return_true()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("something.TEST.ENV.sql", environment).ShouldBeTrue();
            }

            [Observation]
            public void if_given_PROD_and_in_TEST_should_return_false()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("PROD.something.ENV.sql", environment).ShouldBeFalse();
            }

            [Observation]
            public void if_given_BOBTEST_at_the_front_and_in_TEST_should_return_false()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("BOBTEST.something.ENV.sql", environment).ShouldBeFalse();
            }

            [Observation]
            public void if_given_BOBTEST_in_the_middle_and_in_TEST_should_return_false()
            {
                sut.this_is_an_environment_file_and_its_in_the_right_environment("something.BOBTEST.ENV.sql", environment).ShouldBeFalse();
            }
        }

        [Concern(typeof(DefaultDatabaseMigrator))]
        public class when_determining_if_we_are_running_an_everytime_script : concern_for_database_migrator
        {
            public override void Because() { }

            [Observation]
            public void if_given_EVERYTIME_at_the_front_it_should_return_true()
            {
                sut.this_is_an_every_time_script("EVERYTIME.something.sql", false).ShouldBeTrue();
            }

            [Observation]
            public void if_given_EVERYTIME_in_the_middle_it_should_return_true()
            {
                sut.this_is_an_every_time_script("something.EVERYTIME.sql", false).ShouldBeTrue();
            }

            [Observation]
            public void if_passed_true_for_run_everytime_it_should_return_true_no_matter_what_the_name_of_the_script_is()
            {
                sut.this_is_an_every_time_script("something.sql", true).ShouldBeTrue();
            }

            [Observation]
            public void if_given_TIME_it_should_return_false()
            {
                sut.this_is_an_every_time_script("something.TIME.sql", false).ShouldBeFalse();
            }

        }


    }
}