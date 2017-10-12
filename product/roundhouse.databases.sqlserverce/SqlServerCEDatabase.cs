using System.Data.SqlServerCe;

namespace roundhouse.databases.sqlserverce
{
    using System;
    using System.IO;
    using System.Data;
    using System.Security.Principal;
    using infrastructure.app;
    using infrastructure.logging;
    using roundhouse.connections;
    using infrastructure.filesystem;

    /// <summary>
    /// Provider for SQL Server Compact Edition 4.0 databases. 
    /// 
    /// These don't have a master database so if you perform operations under 
    /// the admin connection it is the same as the default. 
    /// 
    /// Additionally, the RoundhousE tables use "Roundhouse_" 
    /// as a prefix instead of a schema as there are none in SQL Server Compact Edition.
    /// 
    /// Use the filename as the server parameter in the command-line console.
    /// </summary>
    public class SqlServerCEDatabase : AdoNetDatabase
    {
        private string connect_options = "Persist Security Info=False";

        public SqlServerCEDatabase()
        {
            version_table_name = "RoundhousE_Version";
            scripts_run_table_name = "RoundhousE_ScriptsRun";
            scripts_run_errors_table_name = "RoundhousE_ScriptsRunErrors";
            command_timeout = 0;
            admin_command_timeout = 0;
        }

        public override string sql_statement_separator_regex_pattern
        {
            get
            {
                const string strings = @"(?<KEEP1>'[^']*')";
                const string dashComments = @"(?<KEEP1>--.*$)";
                const string starComments = @"(?<KEEP1>/\*[\S\s]*?\*/)";
                const string separator = @"(?<KEEP1>\s)(?<BATCHSPLITTER>GO)(?<KEEP2>\s|$)";
                return strings + "|" + dashComments + "|" + starComments + "|" + separator;
            }
        }

        public override void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (!string.IsNullOrEmpty(connection_string))
            {
                var connection_string_builder = new SqlCeConnectionStringBuilder(connection_string);
                server_name = connection_string_builder.DataSource;
            }

            if (string.IsNullOrEmpty(connection_string))
            {
                connection_string = build_connection_string(server_name, connect_options);
                admin_connection_string = connection_string;
            }

            configuration_property_holder.ConnectionString = connection_string;
            configuration_property_holder.ConnectionStringAdmin = connection_string;

            set_provider();
        }

        public override void set_provider()
        {
            provider = "System.Data.SqlServerCe.4.0";
        }

        private static string build_connection_string(string server_name, string connection_options)
        {
            return string.Format("data source={0};{1}", server_name, connection_options);
        }

        protected override void connection_specific_setup(IDbConnection connection)
        {
            //((SqlCeConnection)connection).InfoMessage += (sender, e) => Log.bound_to(this).log_a_debug_event_containing("  [SQL PRINT]: {0}{1}", Environment.NewLine, e.Message);
        }

        public override void open_admin_connection()
        {
            Log.bound_to(this).log_a_debug_event_containing("Opening admin connection to '{0}'", connection_string);
            server_connection = new AdoNetConnection(new SqlCeConnection(connection_string));
            server_connection.open();
        }

        public override void open_connection(bool with_transaction)
        {
            Log.bound_to(this).log_a_debug_event_containing("Opening connection to '{0}'", connection_string);
            server_connection = new AdoNetConnection(new SqlCeConnection(connection_string));
            server_connection.open();

            if (with_transaction)
            {
                transaction = server_connection.underlying_type().BeginTransaction();
            }

            set_repository();
            if (repository != null)
            {
                repository.start(with_transaction);
            }
        }

        public override void run_database_specific_tasks()
        {
        }

        public override string create_database_script()
        {
            return string.Empty;
        }

        public string create_roundhouse_schema_script()
        {
            return string.Empty;
        }

        public override bool create_database_if_it_doesnt_exist(string custom_create_database_script)
        {
            try
            {
                SqlCeEngine engine = new SqlCeEngine(this.connection_string);
                engine.CreateDatabase();
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Unable to open file at {0}, error was: {1}", server_name, ex.Message));
            }

            return true;
        }

        public override void delete_database_if_it_exists()
        {
            if (File.Exists(server_name))
            {
                File.Delete(server_name);
            }
        }

        public override string set_recovery_mode_script(bool simple)
        {
            return string.Empty;
        }

        public override string restore_database_script(string restore_from_path, string custom_restore_options)
        {
            return string.Empty;
        }

        public override string delete_database_script()
        {
            return string.Empty;
        }

        public override void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            SqlCeConnection conn = null;

            try
            {
                using (conn = new SqlCeConnection(connection_string))
                {
                    conn.Open();

                    DateTime now = DateTime.Now;

                    using (SqlCeCommand cmd = conn.CreateCommand())
                    {
                        cmd.Parameters.AddWithValue("version_id", version_id);
                        cmd.Parameters.AddWithValue("script_name", script_name);
                        cmd.Parameters.AddWithValue("sql_to_run", ((object)sql_to_run) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("sql_to_run_hash", ((object)sql_to_run_hash) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("run_this_script_once", run_this_script_once);
                        cmd.Parameters.AddWithValue("now", now);
                        cmd.Parameters.AddWithValue("currentUser", GetCurrentUser());

                        cmd.CommandText = "INSERT INTO [RoundhousE_ScriptsRun]" +
                                          "([version_id]" +
                                          ",[script_name]" +
                                          ",[text_of_script]" +
                                          ",[text_hash]" +
                                          ",[one_time_script]" +
                                          ",[entry_date]" +
                                          ",[modified_date]" +
                                          ",[entered_by])" +
                                          " VALUES(" +
                                          " @version_id " +
                                          ", @script_name " +
                                          ", @sql_to_run " +
                                          ", @sql_to_run_hash " +
                                          ", @run_this_script_once " +
                                          ", @now " +
                                          ", @now " +
                                          ", @currentUser)";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Unable to insert row in RoundhousE_ScriptsRun table. Error was {0}", ex.Message));
            }
        }

        public override void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string repository_version,
                                            string repository_path)
        {
            SqlCeConnection conn = null;

            try
            {
                using (conn = new SqlCeConnection(connection_string))
                {
                    conn.Open();

                    DateTime now = DateTime.Now;

                    using (SqlCeCommand cmd = conn.CreateCommand())
                    {
                        cmd.Parameters.AddWithValue("script_name", script_name);
                        cmd.Parameters.AddWithValue("sql_to_run", ((object)sql_to_run) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("sql_erroneous_part", ((object)sql_erroneous_part) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("error_message", error_message);
                        cmd.Parameters.AddWithValue("repository_version", repository_version);
                        cmd.Parameters.AddWithValue("repository_path", ((object)repository_path) ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("now", now);
                        cmd.Parameters.AddWithValue("currentUser", GetCurrentUser());

                        cmd.CommandText = "INSERT INTO [RoundhousE_ScriptsRunErrors]" +
                                          "([repository_path]" +
                                          ",[version]" +
                                          ",[script_name]" +
                                          ",[text_of_script]" +
                                          ",[erroneous_part_of_script]" +
                                          ",[error_message]" +
                                          ",[entry_date]" +
                                          ",[modified_date]" +
                                          ",[entered_by])" +
                                          " VALUES(" +
                                          "@repository_path " +
                                          ", @repository_version " +
                                          ", @script_name " +
                                          ", @sql_to_run " +
                                          ", @sql_erroneous_part " +
                                          ", @error_message " +
                                          ", @now " +
                                          ", @now " +
                                          ", @currentUser)";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Unable to insert row in RoundhousE_ScriptsRunErrors table. Error was {0}", ex.Message));
            }
        }

        public override string get_version(string repository_path)
        {
            SqlCeConnection conn = null;

            try
            {
                using (conn = new SqlCeConnection(connection_string))
                {
                    conn.Open();

                    DateTime now = DateTime.Now;

                    using (SqlCeCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = "SELECT TOP 1 [Version] FROM [RoundhousE_Version] ORDER BY entry_date DESC";

                        object version = cmd.ExecuteScalar();

                        return version == null ? null : version.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Unable to get version from RoundhousE_Version table. Error was {0}", ex.Message));
            }
        }

        public override long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            string version = get_version(null);

            long lVersion = version == null ? -1 : Int64.Parse(version);

            long lNewVersion = Int64.Parse(repository_version);

            SqlCeConnection conn = null;

            try
            {
                using (conn = new SqlCeConnection(connection_string))
                {
                    conn.Open();

                    DateTime now = DateTime.Now;

                    if (lNewVersion > lVersion)
                    {
                        using (SqlCeCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.AddWithValue("repository_version", repository_version);
                            cmd.Parameters.AddWithValue("repository_path", ((object)repository_path) ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("now", now);
                            cmd.Parameters.AddWithValue("currentUser", GetCurrentUser());

                            cmd.CommandText = "INSERT INTO [RoundhousE_Version]" +
                                                "([repository_path]" +
                                                ",[version]" +
                                                ",[entry_date]" +
                                                ",[modified_date]" +
                                                ",[entered_by])" +
                                                " VALUES(" +
                                                "@repository_path " +
                                                ", @repository_version " +
                                                ", @now " +
                                                ", @now " +
                                                ", @currentUser)";
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (SqlCeCommand cmdLatestVersionId = conn.CreateCommand())
                    {
                        cmdLatestVersionId.CommandText = "SELECT TOP 1 [id] FROM [RoundhousE_Version] ORDER BY entry_date DESC";

                        return Int64.Parse(cmdLatestVersionId.ExecuteScalar().ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Unable to insert new version in RoundhousE_Version table. Error was {0}", ex.Message));
            }
        }

        public override string get_current_script_hash(string script_name)
        {
            SqlCeConnection conn = null;

            try
            {
                using (conn = new SqlCeConnection(connection_string))
                {
                    conn.Open();

                    DateTime now = DateTime.Now;

                    using (SqlCeCommand cmd = conn.CreateCommand())
                    {
                        cmd.Parameters.AddWithValue("script_name", script_name);

                        cmd.CommandText = "SELECT [text_hash] FROM [RoundhousE_ScriptsRun] WHERE script_name = @script_name";

                        object text_hash = cmd.ExecuteScalar();

                        return text_hash == null ? null : text_hash.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException(string.Format("Unable to retrieve script_hash from RoundhousE_ScriptsRun table. Error was {0}", ex.Message));
            }
        }

        public override bool has_run_script_already(string script_name)
        {
            return get_current_script_hash(script_name) != null;
        }

        public override void create_or_update_roundhouse_tables()
        {
            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].", version_table_name);

            command_timeout = 0;

            run_sql("CREATE TABLE [RoundhousE_Version](" +
                    "[id] [bigint] IDENTITY(1,1) PRIMARY KEY, " +
                    "[repository_path] [nvarchar](255) NULL, " +
                    "[version] [nvarchar](50) NULL, " +
                    "[entry_date] [datetime] NULL, " +
                    "[modified_date] [datetime] NULL, " +
                    "[entered_by] [nvarchar](50) NULL)", ConnectionType.Default);

            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].", scripts_run_table_name);

            run_sql("CREATE TABLE [RoundhousE_ScriptsRun](" +
                    "[id] [bigint] IDENTITY(1,1) PRIMARY KEY," +
                    "[version_id] [bigint] NULL," +
                    "[script_name] [nvarchar](255) NULL," +
                    "[text_of_script] [ntext] NULL," +
                    "[text_hash] [nvarchar](512) NULL," +
                    "[one_time_script] [bit] NULL," +
                    "[entry_date] [datetime] NULL," +
                    "[modified_date] [datetime] NULL," +
                    "[entered_by] [nvarchar](50) NULL)", ConnectionType.Default);

            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].", scripts_run_errors_table_name);

            run_sql("CREATE TABLE [RoundhousE_ScriptsRunErrors](" +
                    "[id] [bigint] IDENTITY(1,1) PRIMARY KEY," +
                    "[repository_path] [nvarchar](255) NULL," +
                    "[version] [nvarchar](50) NULL," +
                    "[script_name] [nvarchar](255) NULL," +
                    "[text_of_script] [ntext] NULL," +
                    "[erroneous_part_of_script] [ntext] NULL," +
                    "[error_message] [ntext] NULL," +
                    "[entry_date] [datetime] NULL," +
                    "[modified_date] [datetime] NULL," +
                    "[entered_by] [nvarchar](50) NULL)", ConnectionType.Default);
        }

        /// <summary>
        /// Low level hit to query the database for a restore
        /// </summary>
        private DataTable execute_datatable(string sql_to_run)
        {
            DataSet result = new DataSet();

            using (IDbCommand command = setup_database_command(sql_to_run, ConnectionType.Default, null))
            {
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

        /// <summary>
        /// Returns the currently logged in Windows user.
        /// </summary>
        /// <returns>The current user or an empty string if not available.</returns>
        private string GetCurrentUser()
        {
            return WindowsIdentity.GetCurrent() != null ? WindowsIdentity.GetCurrent().Name : string.Empty;
        }
    }
}