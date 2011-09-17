namespace roundhouse.tests.integration.databases
{
    using bdddoc.core;
    using developwithpassion.bdd.contexts;
    using developwithpassion.bdd.mbunit;
    using developwithpassion.bdd.mbunit.standard;
    using developwithpassion.bdd.mbunit.standard.observations;
    using roundhouse.databases.mysql;
    using roundhouse.databases.sqlserver;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.logging.custom;

    public class SqlServerDatabaseSpecs
    {
        public abstract class concern_for_SqlServerDatabase : observations_for_a_static_sut
        {
            protected static string database_name = "TestRoundhousE";
            protected static string sql_files_folder = @"..\..\..\..\db\SqlServer\TestRoundhousE";

            private after_all_observations after = () =>
                                                   {
                                                       new Migrate().Set(p =>
                                                                         {
                                                                             p.DatabaseName = database_name;
                                                                             p.SqlFilesDirectory = sql_files_folder;
                                                                             p.Drop = true;
                                                                             p.Silent = true;
                                                                         }).Run();
                                                   };
        }

        [Concern(typeof(SqlServerDatabase))]
        public class when_running_the_migrator_with_sqlserver : concern_for_SqlServerDatabase
        {
            protected static object result;

            private context c = () => { };

            private because b = () =>
                                {
                                    new Migrate().Set(p =>
                                                      {
                                                          p.Logger = new ConsoleLogger();
                                                          p.DatabaseName = database_name;
                                                          p.SqlFilesDirectory = sql_files_folder;
                                                          p.Silent = true;
                                                      }).Run();
                                };

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }
        }  
        
        [Concern(typeof(SqlServerDatabase))]
        public class when_getting_the_default_restore_move_options_for_SqlServer_prior_to_a_restore : concern_for_SqlServerDatabase
        {
            protected static SqlServerDatabase db = new SqlServerDatabase();
            protected static Migrate migrator;

            protected static string result;

            private context c = () => {
                                    migrator = new Migrate().Set(p => {
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
                                };

            private because b = () => {
                                    result = db.get_default_restore_move_options();
                                };

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }

            [Observation]
            public void should_have_the_correct_default_restore_options()
            {
                //NOTE: this is not conclusive since this could vary from system to system depending on where you store stuff. This test needs some work to make it go to the database.
                result.should_be_equal_to(@", MOVE 'TestRoundhousE' TO 'C:\Program Files (x86)\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQL\DATA\TestRoundhousE.mdf', MOVE 'TestRoundhousE_log' TO 'C:\Program Files (x86)\Microsoft SQL Server\MSSQL10.MSSQLSERVER\MSSQL\DATA\TestRoundhousE_log.LDF'");
            }
         
        }
    }
}