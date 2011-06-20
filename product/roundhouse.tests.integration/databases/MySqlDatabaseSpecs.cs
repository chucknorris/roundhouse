namespace roundhouse.tests.integration.databases
{
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.databases.mysql;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;

    public class MySqlDatabaseSpecs
    {
        public abstract class concern_for_MySqlDatabase : observations_for_a_static_sut
        {
            protected static string database_name = "TestRoundhousE";
            protected static string sql_files_folder = @"..\..\..\..\db\MySQL\TestRoundhousE";

            private after_all_observations after = () =>
            {
                new Migrate().Set(p =>
                {
                    p.ConnectionString = "server=localhost;uid=root;database=TestRoundhousE;";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.DatabaseType = "mysql";
                    p.Drop = true;
                    p.Silent = true;
                }).Run();
            };
        }

        [Concern(typeof(MySqlDatabase))]
        public class when_running_the_migrator_with_mysql : concern_for_MySqlDatabase
        {
            protected static object result;

            private context c = () => { };

            private because b = () =>
            {
                new Migrate().Set(p =>
                {
                    p.Logger = new ConsoleLogger();
                    p.ConnectionString = "server=localhost;uid=root;database=TestRoundhousE;";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.DatabaseType = "mysql";
                    p.Silent = true;
                }).Run();
            };

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }
        }
    }
}