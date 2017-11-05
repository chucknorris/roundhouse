using System;

namespace roundhouse.tests.integration.databases
{
    using roundhouse.databases.sqlite;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.logging.custom;

    public class SqLiteDatabaseSpecs    
    {
        public abstract class concern_for_SqLiteDatabase : TinySpec, IDisposable
        {
            protected static string database_name = "TestRoundhousE";
            protected static string sql_files_folder = @"..\..\..\..\db\SQLite\TestRoundhousE";

            public void Dispose()
            {
                new Migrate().Set(p =>
                                  {
                                      p.DatabaseName = database_name;
                                      p.SqlFilesDirectory = sql_files_folder;
                                      p.Drop = true;
                                      p.Silent = true;
                                  }).Run();
            }
        }

        [Concern(typeof(SqliteDatabase))]
        public class when_running_the_migrator_with_sqlite : concern_for_SqLiteDatabase
        {
            protected static object result;

            public override void Context() { }
            public override void Because()
            {
                new Migrate().Set(p =>
                                  {
                                      p.Logger = new ConsoleLogger();
                                      p.DatabaseName = database_name;
                                      p.SqlFilesDirectory = sql_files_folder;
                                      p.Silent = true;
                                  }).Run();
            }

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }
        }

        [Concern(typeof(SqliteDatabase))]
        public class when_getting_the_default_restore_move_options_for_sqlite_prior_to_a_restore : concern_for_SqLiteDatabase
        {
            protected static SqliteDatabase db = new SqliteDatabase();
            protected static Migrate migrator;

            protected static string result;

            public override void Context()
            {
                migrator = new Migrate().Set(p =>
                {
                    p.Logger = new ConsoleLogger();
                    p.DatabaseName = database_name;
                    p.SqlFilesDirectory = sql_files_folder;
                    p.Silent = true;
                });
                migrator.Run();
                ConfigurationPropertyHolder configuration_property_holder = migrator.GetConfiguration();
                ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration_property_holder);
                db.configuration = configuration_property_holder;
                db.server_name = configuration_property_holder.ServerName ?? string.Empty;
                db.database_name = configuration_property_holder.DatabaseName ?? string.Empty;
                db.connection_string = configuration_property_holder.ConnectionString;
                db.admin_connection_string = configuration_property_holder.ConnectionStringAdmin;
                db.roundhouse_schema_name = configuration_property_holder.SchemaName;
                db.version_table_name = configuration_property_holder.VersionTableName;
                db.scripts_run_table_name = configuration_property_holder.ScriptsRunTableName;
                db.scripts_run_errors_table_name = configuration_property_holder.ScriptsRunErrorsTableName;
                db.command_timeout = configuration_property_holder.CommandTimeout;
                db.admin_command_timeout = configuration_property_holder.CommandTimeoutAdmin;
                db.restore_timeout = configuration_property_holder.RestoreTimeout;
                db.initialize_connections(configuration_property_holder);
            }

            public override void Because()
            {
            }

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }

        }
    }
}