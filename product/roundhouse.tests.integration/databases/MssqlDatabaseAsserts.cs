using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using NSpec;

namespace roundhouse.tests.integration.databases
{
    public class MssqlDatabaseAsserts : DatabaseAsserts
    {
        private readonly string database_name;

        public MssqlDatabaseAsserts(string database_name)
        {
            this.database_name = database_name;
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
        public void assert_table_exists(string table_name)
        {
            exec_scalar("select OBJECT_ID(@0)", table_name).should_not_be(DBNull.Value);
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
}