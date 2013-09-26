using System;
using System.IO;
using NSpec;
using roundhouse.databases;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.containers;
using roundhouse.infrastructure.logging.custom;

namespace roundhouse.tests.integration.databases
{
    public class describe_roundhouse_spec : nspec
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

            void when_there_is_no_database()
            {


                context["rh executed with v2 scripts"] = () =>
                    {
                        act = () => new Migrate().Set(p =>
                            {
                                p.DatabaseName = database_name;
                                p.SqlFilesDirectory = sql_files_folder;
                                p.Silent = true;
                            }).Run();
                        it["should create table timmy"] = () =>
                                                          get_assert_database()
                                                              .run_sql_scalar(
                                                                  "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Timmy'",
                                                                  ConnectionType.Default)
                                                              .should_be("Timmy");
                        it["should have at least one scripts in run table"] = () =>
                                                          get_assert_database()
                                                              .run_sql_scalar(
                                                                  "SELECT count(*) FROM RoundhousE.ScriptsRun",
                                                                  ConnectionType.Default)
                                                              .should_not_be(0);

                    };
                context["have empty database"] = () =>
                    {
                        act = () => new Migrate().Set(p =>
                        {
                            p.DatabaseName = database_name;
                            p.SqlFilesDirectory = sql_files_folder_v1;
                            p.Silent = true;
                        }).Run();
                        context["rh executed v2 in dry run mode"] = () =>
                            {
                                act = () => new Migrate().Set(p =>
                                    {
                                        p.DatabaseName = database_name;
                                        p.SqlFilesDirectory = sql_files_folder;
                                        p.Silent = true;
                                        p.DryRun = true;
                                    }).Run();

                                it["should not create table timmy"] = () =>
                                                                      get_assert_database()
                                                                          .run_sql_scalar(
                                                                              "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Timmy'",
                                                                              ConnectionType.Default)
                                                                          .should_be(null);
                                it["should have zero scripts in run table"] = () =>
                                                                              get_assert_database()
                                                                                  .run_sql_scalar(
                                                                                      "SELECT count(*) FROM RoundhousE.ScriptsRun",
                                                                                      ConnectionType.Default)
                                                                                  .should_be(0);
                            };
                        context["rh executed v2 in baseline mode"] = () =>
                            {
                                act = () => new Migrate().Set(p =>
                                    {
                                        p.DatabaseName = database_name;
                                        p.SqlFilesDirectory = sql_files_folder;
                                        p.Silent = true;
                                        p.Baseline = true;
                                    }).Run();
                                it["should not create table timmy"] = () =>
                                                          get_assert_database()
                                                              .run_sql_scalar(
                                                                  "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Timmy'",
                                                                  ConnectionType.Default)
                                                              .should_be(null);
                                it["should have non zero scripts in run table"] = () =>
                                                                  get_assert_database()
                                                                      .run_sql_scalar(
                                                                          "SELECT count(*) FROM RoundhousE.ScriptsRun",
                                                                          ConnectionType.Default)
                                                                      .should_not_be(0);
                            };

                    };
            }


            public void before_each()
            {
                var base_directory = find_scripts_directory(6, "db");
                sql_files_folder = Path.Combine(base_directory, @"SqlServer\TestRoundhousE");
                sql_files_folder_v1 = Path.Combine(base_directory, @"SqlServer\TestRoundhousE_v1");
            }

            public void after_each()
            {
                new Migrate().Set(p =>
                {
                    p.DatabaseName = database_name;
                    p.SqlFilesDirectory = sql_files_folder;
                    p.Drop = true;
                    p.Silent = true;
                }).Run();
            }
            
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
}