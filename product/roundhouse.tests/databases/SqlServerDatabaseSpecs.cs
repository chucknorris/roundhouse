using roundhouse.infrastructure.logging.custom;

namespace roundhouse.tests.databases
{
    using bdddoc.core;
    using consoles;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using log4net;
    using roundhouse.databases;
    using roundhouse.databases.sqlserver;
    using roundhouse.infrastructure.app;

    public class SqlServerDatabaseSpecs
    {
        public abstract class concern_for_SqlServerDatabase : observations_for_a_sut_with_a_contract<Database, SqlServerDatabase>
        {
            protected static ConfigurationPropertyHolder configuration_property_holder;

            private context c = () =>
            {
                configuration_property_holder = new DefaultConfiguration
                {
                    Logger = new Log4NetLogFactory().create_logger_bound_to(typeof (SqlServerDatabaseSpecs))
                };
            };
        }

        [Concern(typeof (SqlServerDatabase))]
        public class when_initializing_a_connection_to_a_sql_server_without_a_connection_string_provided : concern_for_SqlServerDatabase
        {
            private because b = () =>
                                    {
                                        sut.connection_string = "";
                                        sut.database_name = "bob";
                                        sut.server_name = "(local)";
                                        sut.initialize_connections(configuration_property_holder);
                                    };

            [Observation]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                sut.connection_string.should_contain("bob");
            }

            [Observation]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.should_contain("master");
            }

            [Observation]
            public void should_have_local_as_the_server_to_connect_to()
            {
                sut.connection_string.should_contain("(local)");
            }

            [Observation]
            public void should_use_integrated_security_when_a_connection_string_is_not_provided()
            {
                sut.connection_string.should_contain("Integrated Security=SSPI");
            }
        }

        [Concern(typeof (SqlServerDatabase))]
        public class when_initializing_a_connection_to_a_sql_server_with_a_connection_string_provided : concern_for_SqlServerDatabase
        {
            private because b = () =>
                                    {
                                        sut.connection_string = "Server=(local);initial catalog=bob;uid=dude;pwd=123";
                                        sut.database_name = "bob";
                                        sut.server_name = "(local)";
                                        sut.initialize_connections(configuration_property_holder);
                                    };

            [Observation]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                sut.connection_string.should_contain("bob");
            }
            
            [Observation]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.should_contain("master");
            }

            [Observation]
            public void should_have_local_as_the_server_to_connect_to()
            {
                sut.connection_string.should_contain("(local)");
            }

            [Observation]
            public void should_use_connection_string_user_name_password_items_when_provided_in_connection_string()
            {
                sut.connection_string.should_contain("uid=dude");
            }
        }


        [Concern(typeof (SqlServerDatabase))]
        public class when_initializing_a_connection_to_a_sql_server_with_a_database_cased_differently_than_in_the_connection_string :
            concern_for_SqlServerDatabase
        {
            private because b = () =>
                                    {
                                        sut.connection_string = "Server=(local);initial catalog=[boB ad];uid=dude;pwd=123";
                                        sut.database_name = "Bob";
                                        sut.server_name = "(local)";
                                        sut.initialize_connections(configuration_property_holder);
                                    };

            [Observation]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                sut.connection_string.should_contain("boB ad");
            }

            [Observation]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.should_contain("master");
            }

            [Observation]
            public void should_have_local_as_the_server_to_connect_to()
            {
                sut.connection_string.should_contain("(local)");
            }
        }
    }
}