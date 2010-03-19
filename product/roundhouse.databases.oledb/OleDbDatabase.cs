using System;
using System.Data;
using System.Data.OleDb;
using roundhouse.infrastructure.extensions;
using roundhouse.infrastructure.logging;
using roundhouse.sql;

namespace roundhouse.databases.oledb
{
    public class OleDbDatabase : Database
    {
        public string server_name { get; set; }
        public string database_name { get; set; }
        public string provider { get; set; }
        public string connection_string { get; set; }
        public string roundhouse_schema_name { get; set; }
        public string version_table_name { get; set; }
        public string scripts_run_table_name { get; set; }
        public string user_name { get; set; }
        public string sql_statement_separator_regex_pattern
        {
            get { return sql_scripts.separator_characters_regex; }
        }
        public string custom_create_database_script { get; set; }
        public int command_timeout { get; set; }
        public int restore_timeout { get; set; }
        private bool split_batches = true;
        public bool split_batch_statements
        {
            get { return split_batches;  }
            set { split_batches = value;}
        }


        public const string MASTER_DATABASE_NAME = "Master";
        private string connect_options = "Trusted_Connection";
        private OleDbConnection server_connection;
        private OleDbTransaction transaction;
        private bool disposing;
        private SqlScript sql_scripts;

        public void initialize_connection()
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
                connection_string = build_connection_string(server_name, MASTER_DATABASE_NAME, connect_options);
            }

            server_connection = new OleDbConnection(connection_string);
            provider = server_connection.Provider;

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

        public void open_connection(bool with_transaction)
        {
            server_connection.Open();

            if (with_transaction)
            {
                transaction = server_connection.BeginTransaction();
            }
        }

        public void close_connection()
        {
            if (transaction != null)
            {
                transaction.Commit();
                transaction = null;
            }
            server_connection.Close();
        }

        public void create_database_if_it_doesnt_exist()
        {
            try
            {
                string create_script = sql_scripts.create_database(database_name);
                if (!string.IsNullOrEmpty(custom_create_database_script))
                {
                    create_script = custom_create_database_script;
                }

                run_sql(create_script);
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for creating a database at this time.",
                    provider);
            }
        }

        public void set_recovery_mode(bool simple)
        {
            try
            {
                run_sql(sql_scripts.set_recovery_mode(database_name, simple));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for setting recovery mode to simple at this time.",
                    provider);
            }
        }

        public void backup_database(string output_path_minus_database)
        {
            //Log.bound_to(this).log_a_warning_event_containing("OleDB with provider {0} does not provide a facility for backing up a database at this time.", provider);
        }

        public void restore_database(string restore_from_path, string custom_restore_options)
        {
            try
            {
                int current_timeout = command_timeout;
                command_timeout = restore_timeout;
                run_sql(sql_scripts.restore_database(database_name, restore_from_path, custom_restore_options));
                command_timeout = current_timeout;
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for restoring a database at this time.",
                    provider);
            }
        }

        public void delete_database_if_it_exists()
        {
            try
            {
                run_sql(sql_scripts.delete_database(database_name));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for deleting a database at this time.",
                    provider);
            }
        }

        public void use_database(string database_name)
        {
            try
            {
                run_sql(sql_scripts.use_database(database_name));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for transfering to a database name at this time.",
                    provider);
            }
        }

        public void create_roundhouse_schema_if_it_doesnt_exist()
        {
            try
            {
                run_sql(sql_scripts.create_roundhouse_schema(roundhouse_schema_name));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "Either the schema has already been created OR OleDB with provider {0} does not provide a facility for creating roundhouse schema at this time.",
                    provider);
            }
        }

        public void create_roundhouse_version_table_if_it_doesnt_exist()
        {
            try
            {
                run_sql(sql_scripts.create_roundhouse_version_table(roundhouse_schema_name, version_table_name));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "Either the version table has already been created OR OleDB with provider {0} does not provide a facility for creating roundhouse version table at this time.",
                    provider);
            }
        }

        public void create_roundhouse_scripts_run_table_if_it_doesnt_exist()
        {
            try
            {
                run_sql(sql_scripts.create_roundhouse_scripts_run_table(roundhouse_schema_name, version_table_name,
                                                                        scripts_run_table_name));
            }
            catch (Exception)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "Either the scripts run table has already been created OR OleDB with provider {0} does not provide a facility for creating roundhouse scripts run table at this time.",
                    provider);
            }
        }

        public void run_sql(string sql_to_run)
        {
            if (string.IsNullOrEmpty(sql_to_run)) return;

            using (OleDbCommand command = server_connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql_to_run;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = command_timeout;
                command.ExecuteNonQuery();
                command.Dispose();
            }
        }

        public void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            try
            {
                run_sql(sql_scripts.insert_script_run(roundhouse_schema_name, scripts_run_table_name, version_id,
                                                      script_name,
                                                      sql_to_run, sql_to_run_hash, run_this_script_once, user_name));
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for recording scripts run at this time.",
                    provider);
            }
        }

        public string get_version(string repository_path)
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

        public long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            long version_id = 0;
            try
            {
                run_sql(sql_scripts.insert_version(roundhouse_schema_name, version_table_name, repository_path, repository_version, user_name));
                string version_id_temp = run_sql_scalar(sql_scripts.get_version_id(roundhouse_schema_name, version_table_name, repository_path)).ToString();

                long.TryParse(version_id_temp, out version_id);
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "OleDB with provider {0} does not provide a facility for inserting versions at this time.", provider);
            }

            return version_id;
        }

        public bool has_run_script_already(string script_name)
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

        public string get_current_script_hash(string script_name)
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

        public object run_sql_scalar(string sql_to_run)
        {
            object return_value = new object();

            if (string.IsNullOrEmpty(sql_to_run)) return return_value;

            using (OleDbCommand command = server_connection.CreateCommand())
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

            using (OleDbCommand command = server_connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql_to_run;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = command_timeout;
                using (OleDbDataReader data_reader = command.ExecuteReader())
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

        public void Dispose()
        {
            if (!disposing)
            {
                server_connection.Dispose();
                disposing = true;
            }
        }
    }
}