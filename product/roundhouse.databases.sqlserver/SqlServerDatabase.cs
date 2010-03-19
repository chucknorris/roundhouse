namespace roundhouse.databases.sqlserver
{
    using System.Data;
    using System.Data.SqlClient;
    using infrastructure.extensions;
    using sql;

    public class SqlServerDatabase : Database
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
            get { return split_batches; }
            set { split_batches = value; }
        }

        public const string MASTER_DATABASE_NAME = "Master";
        private string connect_options = "Integrated Security";
        private SqlConnection server_connection;
        private SqlTransaction transaction;
        private bool disposing;
        private SqlScript sql_scripts;

        public void initialize_connection()
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

            if (connect_options == "Integrated Security")
            {
                connect_options = "Integrated Security=SSPI;";
            }

            if (string.IsNullOrEmpty(connection_string) || connection_string.to_lower().Contains(database_name.to_lower()))
            {
                connection_string = build_connection_string(server_name, MASTER_DATABASE_NAME, connect_options);
            }

            server_connection = new SqlConnection(connection_string);
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
            use_database(MASTER_DATABASE_NAME);
            string create_script = sql_scripts.create_database(database_name);
            if (!string.IsNullOrEmpty(custom_create_database_script))
            {
                create_script = custom_create_database_script;
            }

            run_sql(create_script);
        }

        public void set_recovery_mode(bool simple)
        {
            use_database(MASTER_DATABASE_NAME);
            run_sql(sql_scripts.set_recovery_mode(database_name, simple));
        }

        public void backup_database(string output_path_minus_database)
        {
            //todo: backup database is not a script - it is a command
            //Server sql_server =
            //    new Server(new ServerConnection(new SqlConnection(build_connection_string(server_name, database_name))));
            //sql_server.BackupDevices.Add(new BackupDevice(sql_server,database_name));
        }

        public void restore_database(string restore_from_path, string custom_restore_options)
        {
            use_database(MASTER_DATABASE_NAME);

            int current_timeout = command_timeout;
            command_timeout = restore_timeout;
            run_sql(sql_scripts.restore_database(database_name, restore_from_path, custom_restore_options));
            command_timeout = current_timeout;
        }

        public void delete_database_if_it_exists()
        {
            use_database(MASTER_DATABASE_NAME);
            run_sql(sql_scripts.delete_database(database_name));
        }

        public void use_database(string database_name)
        {
            run_sql(sql_scripts.use_database(database_name));
        }

        public void create_roundhouse_schema_if_it_doesnt_exist()
        {
            run_sql(sql_scripts.create_roundhouse_schema(roundhouse_schema_name));
        }

        public void create_roundhouse_version_table_if_it_doesnt_exist()
        {
            run_sql(sql_scripts.create_roundhouse_version_table(roundhouse_schema_name, version_table_name));
        }

        public void create_roundhouse_scripts_run_table_if_it_doesnt_exist()
        {
            run_sql(sql_scripts.create_roundhouse_scripts_run_table(roundhouse_schema_name, version_table_name, scripts_run_table_name));
        }

        public void run_sql(string sql_to_run)
        {
            if (string.IsNullOrEmpty(sql_to_run)) return;

            using (SqlCommand command = server_connection.CreateCommand())
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
            run_sql(sql_scripts.insert_script_run(roundhouse_schema_name, scripts_run_table_name, version_id, script_name, sql_to_run, sql_to_run_hash,
                                                  run_this_script_once, user_name));
        }

        public string get_version(string repository_path)
        {
            return (string) run_sql_scalar(sql_scripts.get_version(roundhouse_schema_name, version_table_name, repository_path));
        }

        public long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            run_sql(sql_scripts.insert_version(roundhouse_schema_name, version_table_name, repository_path, repository_version, user_name));
            return (long) run_sql_scalar(sql_scripts.get_version_id(roundhouse_schema_name, version_table_name, repository_path));
        }

        public string get_current_script_hash(string script_name)
        {
            return (string) run_sql_scalar(sql_scripts.get_current_script_hash(roundhouse_schema_name, scripts_run_table_name, script_name));
        }

        public bool has_run_script_already(string script_name)
        {
            bool script_has_run = false;

            DataTable data_table = execute_datatable(sql_scripts.has_script_run(roundhouse_schema_name, scripts_run_table_name, script_name));
            if (data_table.Rows.Count > 0)
            {
                script_has_run = true;
            }

            return script_has_run;
        }

        public object run_sql_scalar(string sql_to_run)
        {
            object return_value = new object();

            if (string.IsNullOrEmpty(sql_to_run)) return return_value;

            using (SqlCommand command = server_connection.CreateCommand())
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

            using (SqlCommand command = server_connection.CreateCommand())
            {
                command.Transaction = transaction;
                command.CommandText = sql_to_run;
                command.CommandType = CommandType.Text;
                command.CommandTimeout = command_timeout;
                using (SqlDataReader data_reader = command.ExecuteReader())
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