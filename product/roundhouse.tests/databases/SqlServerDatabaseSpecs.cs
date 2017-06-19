using roundhouse.infrastructure.logging.custom;
using Should;

namespace roundhouse.tests.databases
{
    using consoles;
    using roundhouse.databases.sqlserver;
    using roundhouse.infrastructure.app;

    public class SqlServerDatabaseSpecs
    {
        // ReSharper disable once InconsistentNaming
        public abstract class concern_for_SqlServerDatabase : TinySpec<SqlServerDatabase>
        {
            protected static ConfigurationPropertyHolder configuration_property_holder;

            protected override SqlServerDatabase sut { get; } = new SqlServerDatabase();

            public override void Context() 
            {
                configuration_property_holder = new DefaultConfiguration
                {
                    Logger = new Log4NetLogFactory().create_logger_bound_to(typeof (SqlServerDatabaseSpecs))
                };
            }
        }

        [Concern(typeof (SqlServerDatabase))]
        public class when_initializing_a_connection_to_a_sql_server_without_a_connection_string_provided : concern_for_SqlServerDatabase
        {
            public override void Because()
                                    {
                                        sut.connection_string = "";
                                        sut.database_name = "bob";
                                        sut.server_name = "(local)";
                                        sut.initialize_connections(configuration_property_holder);
                                    }

            [Observation]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                sut.connection_string.ShouldContain("bob");
            }

            [Observation]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.ShouldContain("master");
            }

            [Observation]
            public void should_have_local_as_the_server_to_connect_to()
            {
                sut.connection_string.ShouldContain("(local)");
            }

            [Observation]
            public void should_use_integrated_security_when_a_connection_string_is_not_provided()
            {
                sut.connection_string.ShouldContain("Integrated Security=SSPI");
            }
        }

        [Concern(typeof(SqlServerDatabase))]
        public class when_initializing_a_connection_to_a_sql_server_with_a_connection_string_provided : concern_for_SqlServerDatabase
        {
            public override void Because()
                                    {
                                        sut.connection_string = "Server=(local);initial catalog=bob;uid=dude;pwd=123";
                                        sut.database_name = "bob";
                                        sut.server_name = "(local)";
                                        sut.initialize_connections(configuration_property_holder);
                                    }

            [Observation]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                sut.connection_string.ShouldContain("bob");
            }
            
            [Observation]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.ShouldContain("master");
            }

            [Observation]
            public void should_have_local_as_the_server_to_connect_to()
            {
                sut.connection_string.ShouldContain("(local)");
            }

            [Observation]
            public void should_use_connection_string_user_name_password_items_when_provided_in_connection_string()
            {
                sut.connection_string.ShouldContain("uid=dude");
            }
        }


        [Concern(typeof (SqlServerDatabase))]
        public class when_initializing_a_connection_to_a_sql_server_with_a_database_cased_differently_than_in_the_connection_string :
            concern_for_SqlServerDatabase
        {
            public override void  Because()
                                    {
                                        sut.connection_string = "Server=(local);initial catalog=[boB ad];uid=dude;pwd=123";
                                        sut.database_name = "Bob";
                                        sut.server_name = "(local)";
                                        sut.initialize_connections(configuration_property_holder);
                                    }

            [Observation]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                sut.connection_string.ShouldContain("boB ad");
            }

            [Observation]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.ShouldContain("master");
            }

            [Observation]
            public void should_have_local_as_the_server_to_connect_to()
            {
                sut.connection_string.ShouldContain("(local)");
            }
        }

        [Concern(typeof (SqlServerDatabase))]
        public class when_initializing_a_connection_to_a_sql_azure_database :
            concern_for_SqlServerDatabase
        {
            public override void Because()
            {
                sut.connection_string =
                    "Server=randomsymbols.database.windows.net;Database=bob;User ID=admin@randomsymbols;Password=password;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
                sut.initialize_connections(configuration_property_holder);
            }

            [Observation]
            public void should_have_the_original_server_and_databse_to_connect_to()
            {
                sut.server_name.should_be_equal_to("randomsymbols.database.windows.net");
                sut.database_name.should_be_equal_to("bob");
            }

            [Observation]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.ShouldContain("master");
            }
        }
    }
}