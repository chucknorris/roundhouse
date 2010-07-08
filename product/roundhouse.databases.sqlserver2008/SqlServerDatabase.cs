using System.Text.RegularExpressions;

namespace roundhouse.databases.sqlserver2008
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using infrastructure.extensions;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;
    using parameters;
    using sql;

    public sealed class SqlServerDatabase : DefaultDatabase<Server>
    {
        private string connect_options = "Integrated Security";
        private Server sql_server;
        private Server sql_server_admin;
        private bool running_a_transaction;

        public override void initialize_connection()
        {
            if (!string.IsNullOrEmpty(connection_string))
            {
                string[] parts = connection_string.Split(';');
                foreach (string part in parts)
                {
                    if (string.IsNullOrEmpty(server_name) && (part.to_lower().Contains("server") || part.to_lower().Contains("data source")))
                    {
                        server_name = part.Substring(part.IndexOf("=") + 1);
                    }

                    if (string.IsNullOrEmpty(database_name) && (part.to_lower().Contains("initial catalog") || part.to_lower().Contains("database")))
                    {
                        database_name = part.Substring(part.IndexOf("=") + 1);
                    }
                }

                if (!connection_string.to_lower().Contains(connect_options.to_lower()))
                {
                    connect_options = string.Empty;
                    foreach (string part in parts)
                    {
                        if (!part.to_lower().Contains("server") && !part.to_lower().Contains("data source") && !part.to_lower().Contains("initial catalog") &&
                            !part.to_lower().Contains("database"))
                        {
                            connect_options += part + ";";
                        }
                    }
                }
            }

            master_database_name = "master";
            if (connect_options == "Integrated Security")
            {
                connect_options = "Integrated Security=SSPI;";
            }

            if (string.IsNullOrEmpty(connection_string) || connection_string.to_lower().Contains(database_name.to_lower()))
            {
                connection_string = build_connection_string(server_name, master_database_name, connect_options);
            }

            sql_server = new Server(new ServerConnection(new SqlConnection(connection_string)));

            admin_connection_string = admin_connection_string = Regex.Replace(connection_string, "initial catalog=.*?;", "initial catalog=master;");
            sql_server_admin = new Server(new ServerConnection(new SqlConnection(admin_connection_string)));

            set_provider_and_sql_scripts();
        }

        public override void set_provider_and_sql_scripts()
        {
            provider = "SQLServer";
            SqlScripts.sql_scripts_dictionary.TryGetValue(provider, out sql_scripts);
            if (sql_scripts == null)
            {
                sql_scripts = SqlScripts.t_sql_scripts;
            }
        }

        private static string build_connection_string(string server_name, string database_name, string connection_options)
        {
            return string.Format("Server={0};initial catalog={1};{2}", server_name, database_name, connection_options);
        }

        public override void open_connection(bool with_transaction)
        {
            sql_server.ConnectionContext.Connect();
            if (with_transaction)
            {
                sql_server.ConnectionContext.BeginTransaction();
                running_a_transaction = true;
            }
        }

        public override void close_connection()
        {
            if (running_a_transaction)
            {
                sql_server.ConnectionContext.CommitTransaction();
            }

            sql_server.ConnectionContext.Disconnect();
        }

        public override void open_admin_connection()
        {
            sql_server_admin.ConnectionContext.Connect(); 
        }

        public override void close_admin_connection()
        {
            sql_server_admin.ConnectionContext.Disconnect();
        }

        public override void rollback()
        {
            if (running_a_transaction)
            {
                //rollback previous transaction
                sql_server.ConnectionContext.RollBackTransaction();
                sql_server.ConnectionContext.Disconnect();

                //open a new transaction
                sql_server.ConnectionContext.Connect();
                sql_server.ConnectionContext.BeginTransaction();
                use_database(database_name);
            }
        }

        public override void run_sql(string sql_to_run)
        {
            sql_server.ConnectionContext.StatementTimeout = command_timeout;
            sql_server.ConnectionContext.ExecuteNonQuery(sql_to_run);
        }

        protected override void run_sql(string sql_to_run, IList<IParameter<IDbDataParameter>> parameters)
        {
            throw new NotImplementedException();
        }

        public override void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            run_sql(sql_scripts.insert_script_run(roundhouse_schema_name, scripts_run_table_name, version_id, script_name, sql_to_run, sql_to_run_hash,
                                                  run_this_script_once, user_name));
        }

        public override void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string repository_version, string repository_path)
        {
            run_sql(sql_scripts.insert_script_run_error(roundhouse_schema_name, scripts_run_errors_table_name, repository_version, repository_path, script_name, sql_to_run,
                                                        sql_erroneous_part, error_message, user_name));
        }

        public override string get_version(string repository_path)
        {
            return (string) run_sql_scalar(sql_scripts.get_version(roundhouse_schema_name, version_table_name, repository_path));
        }

        public override long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            run_sql(sql_scripts.insert_version(roundhouse_schema_name, version_table_name, repository_path, repository_version, user_name));
            return (long) run_sql_scalar(sql_scripts.get_version_id(roundhouse_schema_name, version_table_name, repository_path));
        }

        public override string get_current_script_hash(string script_name)
        {
            return (string) run_sql_scalar(sql_scripts.get_current_script_hash(roundhouse_schema_name, scripts_run_table_name, script_name));
        }

        public override bool has_run_script_already(string script_name)
        {
            bool script_has_run = false;

            DataTable data_table = execute_datatable(sql_scripts.has_script_run(roundhouse_schema_name, scripts_run_table_name, script_name));
            if (data_table.Rows.Count > 0)
            {
                script_has_run = true;
            }

            return script_has_run;
        }

        public override object run_sql_scalar(string sql_to_run)
        {
            sql_server.ConnectionContext.StatementTimeout = command_timeout;
            object return_value = sql_server.ConnectionContext.ExecuteScalar(sql_to_run);

            return return_value;
        }

        private DataTable execute_datatable(string sql_to_run)
        {
            sql_server.ConnectionContext.StatementTimeout = command_timeout;
            DataSet result = sql_server.ConnectionContext.ExecuteWithResults(sql_to_run);

            return result.Tables.Count == 0 ? null : result.Tables[0];
        }

        protected override object run_sql_scalar(string sql_to_run, IList<IParameter<IDbDataParameter>> parameters)
        {
            throw new NotImplementedException();
        }

        protected override DataTable execute_datatable(string sql_to_run, IEnumerable<IParameter<IDbDataParameter>> parameters)
        {
            throw new NotImplementedException();
        }

        protected override IParameter<IDbDataParameter> create_parameter(string name, DbType type, object value, int? size)
        {
            throw new NotImplementedException();
        }
    }
}