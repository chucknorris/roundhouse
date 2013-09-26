using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using NSpec;

namespace roundhouse.tests.integration.databases
{
    public class DatabaseAsserts
    {
        private readonly string database_name;

        public DatabaseAsserts(string database_name)
        {
            this.database_name = database_name;
        }

        public void assert_table_exists(string table_name)
        {
            exec_scalar("select OBJECT_ID(@0)", table_name).should_not_be(DBNull.Value);
        }

        private object exec_scalar(string command, params object[] args)
        {
            DbConnection connection = SqlClientFactory.Instance.CreateConnection();
            connection.ConnectionString = "Server=(local);Database=" + database_name + ";Trusted_Connection=True;";
            connection.Open();
            using (connection)
            {
                DbCommand cmd = connection.CreateCommand();
                cmd.CommandText = command;

                if (args.Any())
                {
                    cmd.Parameters.AddRange(args.Select((v, i) =>
                        {
                            DbParameter dbParameter = cmd.CreateParameter();
                            dbParameter.ParameterName = "@" + i;
                            dbParameter.Value = v;
                            return dbParameter;
                        }).ToArray());
                }
                return cmd.ExecuteScalar();
            }
        }

        public void assert_table_not_exists(string table_name)
        {
            exec_scalar("select OBJECT_ID(@0)", table_name).should_be(DBNull.Value);
        }

        public int scripts_run()
        {
            return (int) exec_scalar("SELECT count(*) FROM RoundhousE.ScriptsRun");
        }
    }

    public class describe_roundhouse_spec : nspec
    {
        protected static string database_name = "TestRoundhousE";
        protected static string sql_files_folder_v1;
        protected static string sql_files_folder_v2;

        private static string find_scripts_directory(int iterations, string directory)
            // Hack to locate diredtory root for command line runner and mbunit.gui runner
        {
            if (Directory.Exists(directory))
                return directory;
            if (iterations <= 0)
                throw new Exception("Unable to locate db scripts directory at: " + directory);
            return find_scripts_directory(iterations - 1, Path.Combine("..", directory));
        }

        private void when_there_is_no_database()
        {
            context["rh executed with v2 scripts"] = () =>
                {
                    act = () => new Migrate().Set(p =>
                        {
                            p.DatabaseName = database_name;
                            p.SqlFilesDirectory = sql_files_folder_v2;
                            p.Silent = true;
                        }).Run();
                    it["should create table timmy"] = () =>
                                                      get_assert_database().assert_table_exists("Timmy");
                    it["should create table SampleItems"] = () =>
                                                            get_assert_database().assert_table_exists("SampleItems");
                    specify = () =>
                              get_assert_database()
                                  .scripts_run()
                                  .should_not_be(0);
                };
            context["have v1 database"] = () =>
                {
                    act = () => new Migrate().Set(p =>
                        {
                            p.DatabaseName = database_name;
                            p.SqlFilesDirectory = sql_files_folder_v1;
                            p.Silent = true;
                        }).Run();

                    it["should create table timmy"] = () =>
                                                      get_assert_database().assert_table_exists("Timmy");
                    it["should not create table SampleItems"] = () =>
                                                                get_assert_database()
                                                                    .assert_table_not_exists("SampleItems");
                    specify = () =>
                              get_assert_database().scripts_run().should_be(2);

                    context["rh executed v2 in dry run mode"] = () =>
                        {
                            act = () => new Migrate().Set(p =>
                                {
                                    p.DatabaseName = database_name;
                                    p.SqlFilesDirectory = sql_files_folder_v2;
                                    p.Silent = true;
                                    p.DryRun = true;
                                }).Run();

                            it["should not create table SampleItems"] = () =>
                                                                        get_assert_database()
                                                                            .assert_table_not_exists("SampleItems");
                            specify = () =>
                                      get_assert_database().scripts_run().should_be(2);

                            context["rh executed v2 in normal mode"] = () =>
                                {
                                    act = () => new Migrate().Set(p =>
                                        {
                                            p.DatabaseName = database_name;
                                            p.SqlFilesDirectory = sql_files_folder_v2;
                                            p.Silent = true;
                                        }).Run();
                                    it["should create table SampleItems"] = () =>
                                                                            get_assert_database()
                                                                                .assert_table_exists("SampleItems");


                                    specify = () =>
                                              get_assert_database()
                                                  .scripts_run().should_be(12);
                                };
                        };
                    context["rh executed v2 in baseline mode"] = () =>
                        {
                            act = () => new Migrate().Set(p =>
                                {
                                    p.DatabaseName = database_name;
                                    p.SqlFilesDirectory = sql_files_folder_v2;
                                    p.Silent = true;
                                    p.Baseline = true;
                                }).Run();
                            it["should not create table SampleItems"] = () =>
                                                                        get_assert_database()
                                                                            .assert_table_not_exists("SampleItems");

                            specify = () =>
                                      get_assert_database()
                                          .scripts_run().should_be(13);

                            context["rh executed v2 in normal mode"] = () =>
                                {
                                    act = () => new Migrate().Set(p =>
                                        {
                                            p.DatabaseName = database_name;
                                            p.SqlFilesDirectory = sql_files_folder_v2;
                                            p.Silent = true;
                                        }).Run();
                                    it["should not create table SampleItems"] = () =>
                                                                                get_assert_database()
                                                                                    .assert_table_not_exists(
                                                                                        "SampleItems");


                                    specify = () =>
                                              get_assert_database()
                                                  .scripts_run().should_be(16);
                                };
                        };
                    context["rh executed v2 in normal mode"] = () =>
                        {
                            act = () => new Migrate().Set(p =>
                                {
                                    p.DatabaseName = database_name;
                                    p.SqlFilesDirectory = sql_files_folder_v2;
                                    p.Silent = true;
                                }).Run();
                            it["should create table SampleItems"] = () =>
                                                                    get_assert_database()
                                                                        .assert_table_exists("SampleItems");


                            specify = () =>
                                      get_assert_database()
                                          .scripts_run().should_be(12);
                        };
                };
        }


        public void before_each()
        {
            string base_directory = find_scripts_directory(6, "db");
            sql_files_folder_v2 = Path.Combine(base_directory, @"SqlServer\TestRoundhousE");
            sql_files_folder_v1 = Path.Combine(base_directory, @"SqlServer\TestRoundhousE_v1");
        }

        public void after_each()
        {
            new Migrate().Set(p =>
                {
                    p.DatabaseName = database_name;
                    p.SqlFilesDirectory = sql_files_folder_v2;
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