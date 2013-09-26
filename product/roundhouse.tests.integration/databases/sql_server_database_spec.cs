using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using NSpec;
using roundhouse.connections;
using roundhouse.consoles;
using roundhouse.databases;
using roundhouse.databases.sqlserver;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.containers;
using roundhouse.infrastructure.logging.custom;

namespace roundhouse.tests.integration.databases
{
    public class DatabaseAsserts
    {
        private string database_name;

        public DatabaseAsserts(string database_name)
        {
            this.database_name = database_name;
        }
        public void assert_table_exists(string table_name)
        {
            var connection = SqlClientFactory.Instance.CreateConnection();
            connection.ConnectionString = "Server=(local);Database=" + database_name + ";Trusted_Connection=True;";
            connection.Open();
            using (connection)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "select OBJECT_ID(@id)";
                var dbParameter = cmd.CreateParameter();
                dbParameter.ParameterName = "@id";
                dbParameter.Value = table_name;
                cmd.Parameters.Add(dbParameter);

                var objectId = cmd.ExecuteScalar();
                objectId.should_not_be_null();
            }

        }

        public void assert_table_not_exists(string table_name)
        {
            var connection = SqlClientFactory.Instance.CreateConnection();
            connection.ConnectionString = "Server=(local);Database=" + database_name + ";Trusted_Connection=True;";
            connection.Open();
            using (connection)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "select OBJECT_ID(@id)";
                var dbParameter = cmd.CreateParameter();
                dbParameter.ParameterName = "@id";
                dbParameter.Value = table_name;
                cmd.Parameters.Add(dbParameter);

                var objectId = cmd.ExecuteScalar();
                objectId.should_be(DBNull.Value);
            }

        }

        public int scripts_run()
        {
            var connection = SqlClientFactory.Instance.CreateConnection();
            connection.ConnectionString = "Server=(local);Database=" + database_name + ";Trusted_Connection=True;";
            connection.Open();
            using (connection)
            {
                var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT count(*) FROM RoundhousE.ScriptsRun";

                return (int)cmd.ExecuteScalar();
            }

        }
    }
    public class describe_roundhouse_spec : nspec
    {
        protected static string database_name = "TestRoundhousE";
        protected static string sql_files_folder;
        protected static string sql_files_folder_v1;

        private static string find_scripts_directory(int iterations, string directory) // Hack to locate diredtory root for command line runner and mbunit.gui runner
        {
            if (Directory.Exists(directory))
                return directory;
            if (iterations <= 0)
                throw new Exception("Unable to locate db scripts directory at: " + directory);
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
                                                      get_assert_database().assert_table_exists("Timmy");
                    it["should have at least one scripts in run table"] = () =>
                                                      get_assert_database().scripts_run().should_not_be(0);

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
                                                                  get_assert_database().assert_table_not_exists("Timmy");
                            it["should have zero scripts in run table"] = () =>
                                                                          get_assert_database().scripts_run().should_be(0);
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
                                                                      .assert_table_not_exists("Timmy");

                            it["should have non zero scripts in run table"] = () =>
                                                                get_assert_database()
                                                                      .scripts_run().should_not_be(0);

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

        protected static DatabaseAsserts get_assert_database()
        {
            return new DatabaseAsserts(database_name);
        }

    }
}