namespace roundhouse.tests.databases
{
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.databases;
    using roundhouse.databases.sqlserver2008;

    public class SqlServerDatabaseSpecs
    {
        public abstract class concern_for_SqlServerDatabase : observations_for_a_sut_with_a_contract<Database, SqlServerDatabase>
        {
            private context c = () => { };
        }

        [Concern(typeof (SqlServerDatabase))]
        public class when_initializing_a_connection_to_a_sql_server_without_a_connection_string_provided : concern_for_SqlServerDatabase
        {
            private because b = () =>
                                    {
                                        sut.connection_string = "";
                                        sut.database_name = "bob";
                                        sut.server_name = "(local)";
                                        sut.initialize_connection();
                                    };

            [Observation]
            public void should_have_master_as_the_database_to_connect_to()
            {
                sut.connection_string.should_contain("Master");
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
                                        sut.initialize_connection();
                                    };

            [Observation]
            public void should_have_master_as_the_database_to_connect_to()
            {
                sut.connection_string.should_contain("Master");
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
                                        sut.connection_string = "Server=(local);initial catalog=boB;uid=dude;pwd=123";
                                        sut.database_name = "Bob";
                                        sut.server_name = "(local)";
                                        sut.initialize_connection();
                                    };

            [Observation]
            public void should_have_master_as_the_database_to_connect_to()
            {
                sut.connection_string.should_contain("Master");
            }
        }
    }
}