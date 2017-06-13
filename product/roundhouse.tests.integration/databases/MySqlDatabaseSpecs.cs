using System;
using Xunit;

namespace roundhouse.tests.integration.databases
{
    using roundhouse.infrastructure.logging.custom;

    public class MySqlDatabaseSpecs
    {
        public abstract class concern_for_MySqlDatabase : IDisposable
        {
            protected static string database_name = "TestRoundhousE";
            protected static string sql_files_folder = @"..\..\..\..\db\MySQL\TestRoundhousE";

            public void Dispose()
            {
                new Migrate().Set(p =>
                {
                    p.ConnectionString = "server=localhost;uid=root;database=TestRoundhousE;";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.DatabaseType = "mysql";
                    p.Drop = true;
                    p.Silent = true;
                }).Run();
            }
        }

        public class when_running_the_migrator_with_mysql : concern_for_MySqlDatabase
        {
            public when_running_the_migrator_with_mysql()
            {
                new Migrate().Set(p =>
                {
                    p.Logger = new ConsoleLogger();
                    p.ConnectionString = "server=localhost;uid=root;database=TestRoundhousE;";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.DatabaseType = "mysql";
                    p.Silent = true;
                }).Run();
            }

            [Fact]
            public void should_successfully_run()
            {
                //nothing needed here
            }
        }
    }
}