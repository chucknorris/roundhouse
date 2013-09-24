using System;
using System.IO;
using System.Reflection;
using developwithpassion.bdd.concerns;
using roundhouse.databases;
using roundhouse.infrastructure.app.builders;
using roundhouse.infrastructure.containers;
using roundhouse.infrastructure.filesystem;

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
            protected static string sql_files_folder;
            protected static string sql_files_folder_v1;

            private static string find_scripts_directory(int iterations, string directory) // Hack to locate diredtory root for command line runner and mbunit.gui runner
            {
                if (Directory.Exists(directory))
                    return directory;
                if(iterations <= 0)
                    throw new Exception("Unable to locate db scripts directory at: " +  directory);
                return find_scripts_directory(iterations - 1, Path.Combine("..", directory));
            }

            static concern_for_SqlServerDatabase()
            {
                var base_directory = find_scripts_directory(6, "db");
                sql_files_folder = Path.Combine(base_directory, @"SqlServer\TestRoundhousE");
                sql_files_folder_v1 = Path.Combine(base_directory, @"SqlServer\TestRoundhousE_v1");
            }

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

            protected static Database get_assert_database()
            {
                var m = new Migrate().Set(p =>
                    {
                        p.Logger = new ConsoleLogger();
                        p.DatabaseName = database_name;
                        p.SqlFilesDirectory = sql_files_folder;
                        p.Silent = true;
                        p.DryRun = false;
                    });
                ApplicationConfiguraton.set_defaults_if_properties_are_not_set(m.GetConfiguration());
                ApplicationConfiguraton.build_the_container(m.GetConfiguration());
                return Container.get_an_instance_of<Database>();
            }
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

            [Observation]
            public void should_create_table_timmy()
            {
                get_assert_database().run_sql_scalar("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Timmy'", ConnectionType.Default)
                    .should_be_equal_to("Timmy");
            }

            [Observation]
            public void should_have_at_least_one_scripts_in_run_table()
            {
                get_assert_database().run_sql_scalar("SELECT count(*) FROM RoundhousE.ScriptsRun", ConnectionType.Default)
                    .should_not_be_equal_to(0);
            }

        } 

        [Concern(typeof(SqlServerDatabase))]
        public class when_running_the_migrator_with_sqlserver_in_dry_run_mode : concern_for_SqlServerDatabase
        {
            protected static object result;

            private context c = () =>
                                {
                                    new Migrate().Set(p =>
                                                    {
                                                        p.Logger = new ConsoleLogger();
                                                        p.DatabaseName = database_name;
                                                        p.SqlFilesDirectory = sql_files_folder_v1;
                                                        p.Silent = true;
                                                    }).Run();
                                };

            private because b = () =>
                                {
                                    new Migrate().Set(p =>
                                                      {
                                                          p.Logger = new ConsoleLogger();
                                                          p.DatabaseName = database_name;
                                                          p.SqlFilesDirectory = sql_files_folder;
                                                          p.Silent = true;
                                                          p.DryRun = true;
                                                      }).Run();
                                };

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }

            [Observation]
            public void should_not_create_table_timmy()
            {
                get_assert_database().run_sql_scalar("SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Timmy'", ConnectionType.Default)
                    .should_be_equal_to(0);
            }

            [Observation]
            public void should_have_zero_scripts_in_run_table()
            {
                (get_assert_database().run_sql_scalar("SELECT count(*) FROM RoundhousE.ScriptsRun", ConnectionType.Default))
                    .should_be_equal_to(0);
            }

        }   
        
        [Concern(typeof(SqlServerDatabase))]
        public class when_running_the_migrator_with_sqlserver_in_baseline_mode : concern_for_SqlServerDatabase
        {
            protected static object result;

            private context c = () =>
                                {
                                    new Migrate().Set(p =>
                                                    {
                                                        p.Logger = new ConsoleLogger();
                                                        p.DatabaseName = database_name;
                                                        p.SqlFilesDirectory = sql_files_folder_v1;
                                                        p.Silent = true;
                                                    }).Run();
                                };

            private because b = () =>
                                {
                                    new Migrate().Set(p =>
                                                      {
                                                          p.Logger = new ConsoleLogger();
                                                          p.DatabaseName = database_name;
                                                          p.SqlFilesDirectory = sql_files_folder;
                                                          p.Silent = true;
                                                          p.Baseline = true;
                                                      }).Run();
                                };

            [Observation]
            public void should_successfully_run()
            {
                //nothing needed here
            }

            [Observation]
            public void should_not_create_table_timmy()
            {
                get_assert_database().run_sql_scalar("SELECT count(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Timmy'", ConnectionType.Default)
                    .should_be_equal_to(0);
            }

            [Observation]
            public void should_have_19_scripts_in_run_table()
            {
                (get_assert_database().run_sql_scalar("SELECT count(*) FROM RoundhousE.ScriptsRun", ConnectionType.Default))
                    .should_be_equal_to(19);
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
                var server_data_folder_location =
                    Path.GetDirectoryName(
                        (string)get_assert_database()
                            .run_sql_scalar("SELECT top 1 physical_name FROM sys.database_files", ConnectionType.Default));

                var expected = string.Format(@", MOVE 'TestRoundhousE' TO '{0}\TestRoundhousE.mdf', MOVE 'TestRoundhousE_log' TO '{0}\TestRoundhousE_log.LDF'", server_data_folder_location);
                result.ToLowerInvariant().should_be_equal_to(expected.ToLowerInvariant());
            }
         
        }
    }
}