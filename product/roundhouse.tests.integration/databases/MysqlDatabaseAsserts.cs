using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using MySql.Data.MySqlClient;
using NSpec;

namespace roundhouse.tests.integration.databases
{
    class MysqlDatabaseAsserts : IDatabaseAsserts
    {
        private string database_name;

        public MysqlDatabaseAsserts(string database_name)
        {
            this.database_name = database_name;
        }


        private object exec_scalar(string command, params object[] args)
        {
            DbConnection connection = new MySqlConnection();
            connection.ConnectionString = "server=localhost;uid=root;database=" + database_name;
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

        public void assert_table_exists(string table_name)
        {
            exec_scalar(@"SELECT table_name 
FROM information_schema.tables
WHERE table_schema = @0 
    AND table_name = @1
LIMIT 1;", database_name.ToLower(), table_name.ToLower()).should_not_be(null);
        }


        public void assert_table_not_exists(string table_name)
        {
            exec_scalar(@"SELECT table_name 
FROM information_schema.tables
WHERE table_schema = @0 
    AND table_name = @1
LIMIT 1;", database_name.ToLower(), table_name.ToLower()).should_be(null);
        }

        public int one_time_scripts_run()
        {
            return (int)(long) exec_scalar("SELECT count(*) FROM RoundhousE_ScriptsRun where one_time_script = 1");
        }


    }
}