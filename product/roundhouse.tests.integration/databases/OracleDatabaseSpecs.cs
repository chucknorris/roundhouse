using System;
using roundhouse.databases.oracle;

namespace roundhouse.tests.integration.databases
{
    using roundhouse.databases.mysql;
    using roundhouse.infrastructure.logging.custom;

    public class OracleDatabaseSpecs
    {
        public abstract class concern_for_OracleDatabase :  TinySpec, IDisposable
        {
            protected static string database_name = "TestRoundhousE";
            protected static string sql_files_folder = @"..\..\..\..\db\Oracle\TestRoundhousE";
            
            public void Dispose()
            {
                new Migrate().Set(p =>
                {
                    p.ConnectionString = "Data Source=//localhost/XE;User Id=SYSTEM;Password=123456";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.DatabaseType = "oracle";
                    p.Drop = true;
                    p.DoNotCreateDatabase = true;
                    p.Silent = true;
                }).Run();
            }
        }

        [Concern(typeof(OracleDatabase))]
        public class when_running_the_migrator_with_oracle : concern_for_OracleDatabase
        {
            protected static object result;

            public override void Context() { }
            public override void Because() 
            {
                new Migrate().Set(p =>
                {
                    p.Logger = new ConsoleLogger();
                    p.ConnectionString = "Data Source=//localhost/XE;User Id=SYSTEM;Password=123456";
                    p.SqlFilesDirectory = sql_files_folder;
                    p.DatabaseType = "oracle";
                    p.DoNotCreateDatabase = true;
                    p.Silent = true;
                }).Run();
            }

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }
        }
    }
}