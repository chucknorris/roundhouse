using System;
using System.IO;

namespace roundhouse.tests.integration.databases
{

    using roundhouse.databases.sqlserverce;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.logging.custom;

    public class SqlServerCEDatabaseSpecs
    {
        public abstract class concern_for_SqlServerCEDatabase : TinySpec, IDisposable
        {
            protected readonly string server_name = string.Format("sqlserverce-{0}.sdf", Guid.NewGuid());
            protected static string database_name = "TestRoundhousE";
            protected static string sql_files_folder = @"..\..\..\..\db\SqlServer\TestRoundhousE";
            protected static Migrate migrator;

            protected static SqlServerCEDatabase db = new SqlServerCEDatabase();

            public override void Context()
            {
                lock (server_name)
                {
                    migrator =  new Migrate().Set(p =>
                    {
                        p.ServerName = server_name;
                        p.Logger = new ConsoleLogger();
                        p.DatabaseType = "SqlServerCE";
                        p.SqlFilesDirectory = sql_files_folder;
                        p.Silent = true;
                    });
                    migrator.Run();
                }
            }

            public void Dispose()
            {
                lock (server_name)
                {
                    var i = 1;
                    do
                    {
                        try
                        {
                            delete_database();
                        }
                        catch (IOException)
                        {
                            System.Threading.Thread.Sleep(i * 400);
                        }
                    } while (++i < 5);
                }
            }

            private static void delete_database()
            {
                ConfigurationPropertyHolder configuration_property_holder = migrator.GetConfiguration();
                ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration_property_holder);
                db.configuration = configuration_property_holder;

                db.delete_database_if_it_exists();
            }
        }

        [Concern(typeof(SqlServerCEDatabase))]
        public class when_running_the_migrator_with_SqlServerCE : concern_for_SqlServerCEDatabase
        {
            protected static object result;

            public override void Because() { }

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }
        }

        [Concern(typeof(SqlServerCEDatabase))]
        public class when_getting_the_default_restore_move_options_for_SqlServerCE_prior_to_a_restore : concern_for_SqlServerCEDatabase
        {

            protected static string result;

            public override void Context()
            {
                    base.Context();
                    
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