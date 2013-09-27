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

        public int one_time_scripts_run()
        {
            return (int) exec_scalar("SELECT count(*) FROM RoundhousE.ScriptsRun where one_time_script = 1");
        }
    }

    public class describe_roundhouse_spec : nspec
    {
        protected static string database_name = "TestRoundhousE";
        protected static string sql_files_folder_v1;
        protected static string sql_files_folder_v2;
        private string database_type;
        private string scripts_folder;
        private string connection_string;

        private static string find_scripts_directory(int iterations, string directory)
            // Hack to locate diredtory root for command line runner and mbunit.gui runner
        {
            if (Directory.Exists(directory))
                return directory;
            if (iterations <= 0)
                throw new Exception("Unable to locate db scripts directory at: " + directory);
            return find_scripts_directory(iterations - 1, Path.Combine("..", directory));
        }

        private void _rh(string db_files, bool dry_run = false, bool baseline = false, bool drop = false)
        {
            new Migrate().Set(p =>
                {
                    p.DatabaseName = database_name;
                    p.SqlFilesDirectory = db_files;
                    p.Silent = true;
                    p.DryRun = dry_run;
                    p.Baseline = baseline;
                    p.Drop = drop;
                    //p.ConnectionString = connection_string;
                    //p.DatabaseType = database_type;
                }).Run();
        }

        private void when_mssql()
        {
            database_type = "mssql";
            scripts_folder = "SqlServer";
            connection_string = null;
            //"server=localhost;uid=root;database=TestRoundhousE;"
            DefaultDatabaseTestSuite();
        }

        private void DefaultDatabaseTestSuite()
        {
            context["rh executed with v2 scripts"] = () =>
                {
                    act = () => _rh(sql_files_folder_v2);

                    it["should create table timmy"] = () =>
                                                      get_assert_database().assert_table_exists("Timmy");
                    it["should create table SampleItems"] = () =>
                                                            get_assert_database().assert_table_exists("SampleItems");
                    specify =
                        () => get_assert_database().one_time_scripts_run().should_not_be(0);
                };
            context["have v1 database"] = () =>
                {
                    act = () => _rh(sql_files_folder_v1);

                    it["should create table timmy"] = () =>
                                                      get_assert_database().assert_table_exists("Timmy");
                    it["should not create table SampleItems"] = () =>
                                                                get_assert_database()
                                                                    .assert_table_not_exists("SampleItems");
                    specify = () =>
                              get_assert_database().one_time_scripts_run().should_be(1);

                    context["rh executed v2 in dry run mode"] = () =>
                        {
                            act = () => _rh(sql_files_folder_v2, dry_run: true);

                            it["should not create table SampleItems"] = () =>
                                                                        get_assert_database()
                                                                            .assert_table_not_exists("SampleItems");
                            specify = () =>
                                      get_assert_database().one_time_scripts_run().should_be(1);

                            context["rh executed v2 in normal mode"] = () =>
                                {
                                    act = () => _rh(sql_files_folder_v2);
                                    it["should create table SampleItems"] = () =>
                                                                            get_assert_database()
                                                                                .assert_table_exists("SampleItems");


                                    specify = () =>
                                              get_assert_database()
                                                  .one_time_scripts_run().should_be(4);
                                };
                        };
                    context["rh executed v2 in baseline mode"] = () =>
                        {
                            act = () => _rh(sql_files_folder_v2, baseline: true);
                            it["should not create table SampleItems"] = () =>
                                                                        get_assert_database()
                                                                            .assert_table_not_exists("SampleItems");

                            specify = () =>
                                      get_assert_database()
                                          .one_time_scripts_run().should_be(5);

                            context["rh executed v2 in normal mode"] = () =>
                                {
                                    act = () => _rh(sql_files_folder_v2);
                                    it["should not create table SampleItems"] = () =>
                                                                                get_assert_database()
                                                                                    .assert_table_not_exists(
                                                                                        "SampleItems");


                                    specify = () =>
                                              get_assert_database()
                                                  .one_time_scripts_run().should_be(5);
                                };
                        };
                    context["rh executed v2 in normal mode"] = () =>
                        {
                            act = () => _rh(sql_files_folder_v2);
                            it["should create table SampleItems"] = () =>
                                                                    get_assert_database()
                                                                        .assert_table_exists("SampleItems");


                            specify = () =>
                                      get_assert_database()
                                          .one_time_scripts_run().should_be(4);
                        };
                };
        }


        public void before_each()
        {
            string base_directory = find_scripts_directory(6, "db");
            sql_files_folder_v2 = Path.Combine(base_directory, Path.Combine(scripts_folder, @"TestRoundhousE"));
            sql_files_folder_v1 = Path.Combine(base_directory, Path.Combine(scripts_folder, @"TestRoundhousE_v1"));
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