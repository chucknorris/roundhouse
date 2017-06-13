using System;
using Xunit;

namespace roundhouse.tests.integration.databases
{
    using roundhouse.infrastructure.logging.custom;

    public class MigrateSpecs
    {
        public abstract class concern_for_Migrate : IDisposable
        {
            protected static string database_name = "TestRoundhousE";
            protected static string sql_files_folder = @"..\..\..\..\db\SqlServer\TestRoundhousE";

            public void Dispose()
            {
                new Migrate().Set(p =>
                {
                    p.Logger = new ConsoleLogger(true);
                    p.DatabaseName = "TestRoundhousE";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.Drop = true;
                    p.Silent = true;
                }).Run();
            }
        }

        public class when_running_the_migrator_in_drop_create_mode : concern_for_Migrate
        {
            public when_running_the_migrator_in_drop_create_mode()
            {
                new Migrate().Set(p =>
                {
                    p.Logger = new ConsoleLogger(true);
                    p.DatabaseName = "TestRoundhousE";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.Silent = true;
                }).Run();

                new Migrate().Set(p =>
                {
                    p.Logger = new ConsoleLogger(true);
                    p.DatabaseName = "TestRoundhousE";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.Silent = true;
                }).RunDropCreate();
            }

            [Fact]
            public void should_successfully_run()
            {
                //nothing needed here
            }
        }
    }
}