using System;
using System.Data;
using System.Data.OleDb;
using System.Text.RegularExpressions;
using roundhouse.infrastructure.extensions;
using roundhouse.infrastructure.logging;
using roundhouse.sql;

namespace roundhouse.databases.oledb
{
    using connections;

    public class OleDbDatabase : AdoNetDatabase
    {
        private string connect_options = "Trusted_Connection";

        public override void initialize_connection()
        {
            if (!string.IsNullOrEmpty(connection_string))
            {
                string[] parts = connection_string.Split(';');
                foreach (string part in parts)
                {
                    if (string.IsNullOrEmpty(server_name) && part.to_lower().Contains("server"))
                    {
                        server_name = part.Substring(part.IndexOf("=") + 1);
                    }

                    if (string.IsNullOrEmpty(database_name) && part.to_lower().Contains("database"))
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
            if (connect_options == "Trusted_Connection")
            {
                connect_options = "Trusted_Connection=yes;";
            }

            if (string.IsNullOrEmpty(connection_string) || connection_string.to_lower().Contains(database_name.to_lower()))
            {
                connection_string = build_connection_string(server_name, database_name, connect_options);
            }

            if (connection_string.to_lower().Contains("sqlserver") || connection_string.to_lower().Contains("sqlncli"))
            {
                connection_string = build_connection_string(server_name, master_database_name, connect_options);
            }

            admin_connection_string = Regex.Replace(connection_string, "Database=.*?;", "Database=master;");            

            set_provider_and_sql_scripts();
        }

        public override void open_connection(bool with_transaction)
        {
            server_connection = new AdoNetConnection(new OleDbConnection(connection_string));
            server_connection.open();

            if (with_transaction)
            {
                transaction = server_connection.underlying_type().BeginTransaction();
            }
        }

        public override void open_admin_connection()
        {
            server_connection = new AdoNetConnection(new OleDbConnection(admin_connection_string));
            server_connection.open();
        }

        public override void set_provider_and_sql_scripts()
        {
            provider = ((OleDbConnection)server_connection.underlying_type()).Provider;
            SqlScripts.sql_scripts_dictionary.TryGetValue(provider, out sql_scripts);
            if (sql_scripts == null)
            {
                sql_scripts = SqlScripts.t_sql_scripts;
            }
        }

        private static string build_connection_string(string server_name, string database_name, string connection_options)
        {
            return string.Format("Provider=SQLNCLI;Server={0};Database={1};{2}", server_name, database_name, connection_options);
        }

        public override void run_sql(string sql_to_run)
        {
            if (string.IsNullOrEmpty(sql_to_run)) return;

            using (IDbCommand command = server_connection.underlying_type().CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql_to_run;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = command_timeout;
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }

        //public void run_sql(string sql_to_run, object[] parameters)
        //{
        //    if (string.IsNullOrEmpty(sql_to_run)) return;

        //    using (IDbCommand command = server_connection.underlying_type().CreateCommand())
        //    {
        //        command.Transaction = transaction;
        //        command.CommandText = sql_to_run;
        //        command.CommandType = CommandType.Text;
        //        command.CommandTimeout = command_timeout;
        //        foreach (var parameter in parameters)
        //        {
        //            command.Parameters.Add(parameter);
        //        }
        //        command.ExecuteNonQuery();
        //        command.Dispose();
        //    }
        //}

        public override void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            try
            {
                run_sql(sql_scripts.insert_script_run(roundhouse_schema_name, scripts_run_table_name, version_id,
                                                      script_name,
                                                      sql_to_run, sql_to_run_hash, run_this_script_once, user_name));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for recording scripts run at this time.",
                    provider);
            }
        }

        public override void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string repository_version, string repository_path)
        {
            try
            {
                run_sql(sql_scripts.insert_script_run_error(roundhouse_schema_name, scripts_run_errors_table_name, repository_version, repository_path,
                                                      script_name,
                                                      sql_to_run, sql_erroneous_part, error_message, user_name));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for recording scripts run errors at this time.",
                    provider);
            }
        }

        public override string get_version(string repository_path)
        {
            try
            {
                return (string)run_sql_scalar(sql_scripts.get_version(roundhouse_schema_name, version_table_name, repository_path));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for retrieving versions at this time.",
                    provider);
                return "0";
            }
        }

        public override long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            long version_id = 0;
            try
            {
                run_sql(sql_scripts.insert_version(roundhouse_schema_name, version_table_name, repository_path, repository_version, user_name));
                string version_id_temp = run_sql_scalar(sql_scripts.get_version_id(roundhouse_schema_name, version_table_name, repository_path)).ToString();

                long.TryParse(version_id_temp, out version_id);
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for inserting versions at this time.", provider);
            }

            return version_id;
        }

        public override bool has_run_script_already(string script_name)
        {
            try
            {
                bool script_has_run = false;

                DataTable data_table =
                    execute_datatable(sql_scripts.has_script_run(roundhouse_schema_name, scripts_run_table_name,
                                                                 script_name));
                if (data_table.Rows.Count > 0)
                {
                    script_has_run = true;
                }

                return script_has_run;
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for determining if a script has run at this time.",
                    provider);
                return false;
            }
        }

        public override string get_current_script_hash(string script_name)
        {
            try
            {
                return (string)run_sql_scalar(sql_scripts.get_current_script_hash(roundhouse_schema_name, scripts_run_table_name, script_name));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for hashing (through recording scripts run) at this time.",
                    provider);
                return string.Empty;
            }
        }

        public override object run_sql_scalar(string sql_to_run)
        {
            object return_value = new object();

            if (string.IsNullOrEmpty(sql_to_run)) return return_value;

            using (IDbCommand command = server_connection.underlying_type().CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql_to_run;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = command_timeout;
                return_value = command.ExecuteScalar();
                command.Dispose();
            }

            return return_value;
        }

        private DataTable execute_datatable(string sql_to_run)
        {
            DataSet result = new DataSet();

            using (IDbCommand command = server_connection.underlying_type().CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql_to_run;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = command_timeout;
                using (IDataReader data_reader = command.ExecuteReader())
                {
                    DataTable data_table = new DataTable();
                    data_table.Load(data_reader);
                    data_reader.Close();
                    data_reader.Dispose();

                    result.Tables.Add(data_table);
                }
                command.Dispose();
            }

            return result.Tables.Count == 0 ? null : result.Tables[0];
        }

    }
}