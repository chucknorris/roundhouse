using FluentAssertions;
using roundhouse.infrastructure.logging.custom;
using Xunit;

namespace roundhouse.tests.databases
{
    using consoles;
    using roundhouse.databases;
    using roundhouse.databases.sqlserver;
    using roundhouse.infrastructure.app;

    public class SqlServerDatabaseSpecs
    {
        public abstract class concern_for_SqlServerDatabase 
        {
            protected ConfigurationPropertyHolder configuration_property_holder;
            protected Database sut;

            protected concern_for_SqlServerDatabase()
            {
                sut = new SqlServerDatabase();
                configuration_property_holder = new DefaultConfiguration
                {
                    Logger = new Log4NetLogFactory().create_logger_bound_to(typeof(SqlServerDatabaseSpecs))
                };
                sut.initialize_connections(configuration_property_holder);
            }
        }

        public class when_initializing_a_connection_to_a_sql_server_without_a_connection_string_provided : concern_for_SqlServerDatabase
        {

            public when_initializing_a_connection_to_a_sql_server_without_a_connection_string_provided()
            {
                sut.connection_string = "";
                sut.database_name = "bob";
                sut.server_name = "(local)";
                sut.initialize_connections(configuration_property_holder);
            }

            [Fact]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                sut.connection_string.should_contain("bob");
            }

            [Fact]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.should_contain("master");
            }

            [Fact]
            public void should_have_local_as_the_server_to_connect_to()
            {
                sut.connection_string.should_contain("(local)");
            }

            [Fact]
            public void should_use_integrated_security_when_a_connection_string_is_not_provided()
            {
                sut.connection_string.should_contain("Integrated Security=SSPI");
            }
        }

        public class when_initializing_a_connection_to_a_sql_server_with_a_connection_string_provided : concern_for_SqlServerDatabase
        {
            public when_initializing_a_connection_to_a_sql_server_with_a_connection_string_provided()
            {
                sut.connection_string = "Server=(local);initial catalog=bob;uid=dude;pwd=123";
                sut.database_name = "bob";
                sut.server_name = "(local)";
                sut.initialize_connections(configuration_property_holder);
            }

            [Fact]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                sut.connection_string.should_contain("bob");
            }

            [Fact]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.should_contain("master");
            }

            [Fact]
            public void should_have_local_as_the_server_to_connect_to()
            {
                sut.connection_string.should_contain("(local)");
            }

            [Fact]
            public void should_use_connection_string_user_name_password_items_when_provided_in_connection_string()
            {
                Assert.Contains("uid=dude", sut.connection_string);
            }
        }


        public class when_initializing_a_connection_to_a_sql_server_with_a_database_cased_differently_than_in_the_connection_string :
            concern_for_SqlServerDatabase
        {
            public when_initializing_a_connection_to_a_sql_server_with_a_database_cased_differently_than_in_the_connection_string()
            {
                sut.connection_string = "Server=(local);initial catalog=[boB ad];uid=dude;pwd=123";
                sut.database_name = "Bob";
                sut.server_name = "(local)";
                sut.initialize_connections(configuration_property_holder);
            }

            [Fact]
            public void should_have_the_original_database_as_the_database_to_connect_to()
            {
                Assert.Contains("boB ad", sut.connection_string);
            }

            [Fact]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                Assert.Contains("master", sut.admin_connection_string);
            }

            [Fact]
            public void should_have_local_as_the_server_to_connect_to()
            {
                Assert.Contains("(local)", sut.connection_string);
            }
        }

        public class when_initializing_a_connection_to_a_sql_azure_database :
            concern_for_SqlServerDatabase
        {
            public when_initializing_a_connection_to_a_sql_azure_database()
            {
                sut.connection_string =
                    "Server=randomsymbols.database.windows.net;Database=bob;User ID=admin@randomsymbols;Password=password;Trusted_Connection=False;Encrypt=True;Connection Timeout=30;";
                sut.initialize_connections(configuration_property_holder);
            }

            [Fact]
            public void should_have_the_original_server_and_databse_to_connect_to()
            {
                sut.server_name.Should().Be("randomsymbols.database.windows.net");
                sut.database_name.Should().Be("bob");
            }

            [Fact]
            public void should_have_master_as_the_admin_database_to_connect_to()
            {
                sut.admin_connection_string.should_contain("master");
            }
        }
    }
}