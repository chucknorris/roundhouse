namespace roundhouse.databases
{
    using System;
    using infrastructure.app;
    using infrastructure.persistence;

    public sealed class SqlServerLiteSpeedDatabase : Database
    {
        private readonly Database database;

        public SqlServerLiteSpeedDatabase(Database database)
        {
            this.database = database;
        }

        public ConfigurationPropertyHolder configuration
        {
            get { return database.configuration; }
            set { database.configuration = value; }
        }

        public string connection_string
        {
            get { return database.connection_string; }
            set { database.connection_string = value; }
        }

        public string admin_connection_string
        {
            get { return database.admin_connection_string; }
            set { database.admin_connection_string = value; }
        }

        public string server_name
        {
            get { return database.server_name; }
            set { database.server_name = value; }
        }

        public string database_name
        {
            get { return database.database_name; }
            set { database.database_name = value; }
        }

        public string provider
        {
            get { return database.provider; }
            set { database.provider = value; }
        }

        public string roundhouse_schema_name
        {
            get { return database.roundhouse_schema_name; }
            set { database.roundhouse_schema_name = value; }
        }

        public string version_table_name
        {
            get { return database.version_table_name; }
            set { database.version_table_name = value; }
        }

        public string scripts_run_table_name
        {
            get { return database.scripts_run_table_name; }
            set { database.scripts_run_table_name = value; }
        }

        public string scripts_run_errors_table_name
        {
            get { return database.scripts_run_errors_table_name; }
            set { database.scripts_run_errors_table_name = value; }
        }

        public string user_name
        {
            get { return database.user_name; }
            set { database.user_name = value; }
        }

        public string sql_statement_separator_regex_pattern
        {
            get { return database.sql_statement_separator_regex_pattern; }
        }

        public int command_timeout
        {
            get { return database.command_timeout; }
            set { database.command_timeout = value; }
        }

        public int admin_command_timeout
        {
            get { return database.admin_command_timeout; }
            set { database.admin_command_timeout = value; }
        }

        public int restore_timeout
        {
            get { return database.restore_timeout; }
            set { database.restore_timeout = value; }
        }

        public bool split_batch_statements
        {
            get { return database.split_batch_statements; }
            set { database.split_batch_statements = value; }
        }

        public bool supports_ddl_transactions
        {
            get { return database.supports_ddl_transactions; }
        }

        //public IRepository repository
        //{
        //    get { return database.repository; }
        //    set { database.repository = value; }
        //}

        public void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
        {
            database.initialize_connections(configuration_property_holder);
        }

        public void open_admin_connection()
        {
            database.open_admin_connection();
        }

        public void close_admin_connection()
        {
            database.close_admin_connection();
        }
        public void open_connection(bool with_transaction)
        {
            database.open_connection(with_transaction);
        }

        public void close_connection()
        {
            database.close_connection();
        }

        public void rollback()
        {
            database.rollback();
        }

        public bool create_database_if_it_doesnt_exist(string custom_create_database_script)
        {
            return database.create_database_if_it_doesnt_exist(custom_create_database_script);
        }

        public void set_recovery_mode(bool simple)
        {
            database.set_recovery_mode(simple);
        }

        public void backup_database(string output_path_minus_database)
        {
            database.backup_database(output_path_minus_database);
        }

        public void restore_database(string restore_from_path, string custom_restore_options)
        {
            int current_timeout = command_timeout;
            command_timeout = restore_timeout;
            run_sql(string.Format(
                                 @"USE master 
                        ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        ALTER DATABASE [{0}] SET MULTI_USER;

                        exec master.dbo.xp_restore_database @database = N'{0}',
                            @filename = N'{1}',
                            @filenumber = 1, 
                            @with = N'RECOVERY', 
                            @with = N'NOUNLOAD',
                            @with = N'REPLACE',
                            @with = N'STATS = 10'
                            {2};

                        ALTER DATABASE [{0}] SET MULTI_USER;
                        ALTER DATABASE [{0}] SET RECOVERY SIMPLE;
                        --DBCC SHRINKDATABASE ([{0}]);
                        ",
                         database_name, restore_from_path,
                         string.IsNullOrEmpty(custom_restore_options) ? string.Empty : ", @with = N'" + custom_restore_options.Replace("'", "''") + "'"
                       ),ConnectionType.Admin);
            command_timeout = current_timeout;
        }

        public void delete_database_if_it_exists()
        {
            database.delete_database_if_it_exists();
        }

        public void run_database_specific_tasks()
        {
            database.run_database_specific_tasks();
        }

        public void create_or_update_roundhouse_tables()
        {
            database.create_or_update_roundhouse_tables();
        }

        public void run_sql(string sql_to_run,ConnectionType connection_type)
        {
            database.run_sql(sql_to_run,connection_type);
        }    
        
        public object run_sql_scalar(string sql_to_run,ConnectionType connection_type)
        {
            return database.run_sql_scalar(sql_to_run,connection_type);
        }

        public void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            database.insert_script_run(script_name, sql_to_run, sql_to_run_hash, run_this_script_once, version_id);
        }

        public void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string repository_version, string repository_path)
        {
            database.insert_script_run_error(script_name, sql_to_run, sql_erroneous_part, error_message, repository_version, repository_path);
        }

        public string get_version(string repository_path)
        {
            return database.get_version(repository_path);
        }

        public long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            return database.insert_version_and_get_version_id(repository_path, repository_version);
        }

        public bool has_run_script_already(string script_name)
        {
            return database.has_run_script_already(script_name);
        }

        public string get_current_script_hash(string script_name)
        {
            return database.get_current_script_hash(script_name);
        }

        private bool disposing = false;
        public void Dispose()
        {
            if (!disposing)
            {
                database.Dispose();
                disposing = true;
            }
        }
    }
}