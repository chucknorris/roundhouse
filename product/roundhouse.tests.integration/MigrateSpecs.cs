using System;

namespace roundhouse.tests.integration.databases
{
    using roundhouse.infrastructure.logging.custom;

    public class MigrateSpecs
    {
        public abstract class concern_for_Migrate : TinySpec, IDisposable
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

        [Concern(typeof(Migrate))]
        public class when_running_the_migrator_in_drop_create_mode : concern_for_Migrate
        {
            protected static object result;

            public override void Context() {}

            public override void Because()            {
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

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }
        }
    }
}