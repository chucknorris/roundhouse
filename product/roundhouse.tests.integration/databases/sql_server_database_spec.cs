using System;
using System.IO;
using NSpec;
using roundhouse.databases;
using roundhouse.infrastructure.containers;
using roundhouse.infrastructure.persistence;

namespace roundhouse.tests.integration.databases
{
    public class describe_roundhouse_spec : nspec
    {
        protected static string database_name = "TestRoundhousE";
        protected static string sql_files_folder_v1;
        protected static string sql_files_folder_v2;
        private string database_type;
        private string scripts_folder;
        private string connection_string;
        private DatabaseAsserts assert_database;

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
                    p.DatabaseName = string.IsNullOrEmpty(connection_string) ? database_name : null;
                    p.SqlFilesDirectory = db_files;
                    p.Silent = true;
                    p.DryRun = dry_run;
                    p.Baseline = baseline;
                    p.Drop = drop;
                    p.ConnectionString = connection_string;
                    p.DatabaseType = database_type;
                    p.WithTransaction = false;
                }).Run();
            Container.get_an_instance_of<Database>().Dispose();
        }

        private void when_mssql()
        {
            database_type = "SqlServer";
            scripts_folder = "SqlServer";
            connection_string = null;
            assert_database = new MssqlDatabaseAsserts(database_name);
            //"server=localhost;uid=root;database=TestRoundhousE;"
            DefaultDatabaseTestSuite();
        }

        private void when_mysql()
        {
            database_type = "MySQL";
            scripts_folder = "MySQL";
            connection_string = "server=localhost;uid=root;database=TestRoundhousE;";
            assert_database = new MysqlDatabaseAsserts(database_name);
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
                        () => get_assert_database().one_time_scripts_run().should_be(3);
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
                                                  .one_time_scripts_run().should_be(3);
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
                                          .one_time_scripts_run().should_be(3);

                            context["rh executed v2 in normal mode"] = () =>
                                {
                                    act = () => _rh(sql_files_folder_v2);
                                    it["should not create table SampleItems"] = () =>
                                                                                get_assert_database()
                                                                                    .assert_table_not_exists(
                                                                                        "SampleItems");


                                    specify = () =>
                                              get_assert_database()
                                                  .one_time_scripts_run().should_be(3);
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
                                          .one_time_scripts_run().should_be(3);
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
            _rh(sql_files_folder_v1, drop:true);
        }

        protected DatabaseAsserts get_assert_database()
        {
            return assert_database;
        }
    }
}