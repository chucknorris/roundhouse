namespace roundhouse.databases
{
    using System;
    using infrastructure.app;
    using infrastructure.logging;
    using infrastructure.persistence;

    public class MockDatabase : Database
    {

        private bool database_exists = false;
        private readonly Database database;

        public MockDatabase(Database database)
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

        public void open_connection(bool with_transaction)
        {
            if (database_exists)
            {
                database.open_connection(with_transaction);
            }
        }

        public void close_connection()
        {
            if (database_exists)
            {
                database.close_connection();
            }
        }

        public void open_admin_connection()
        {
            database.open_connection(false);
        }

        public void close_admin_connection()
        {
            database.close_connection();
        }

        public void rollback()
        {
            database.rollback();
        }

        public void create_database_if_it_doesnt_exist(string custom_create_database_script)
        {
            //TODO: Don't allow creation of the database - record everything from here on out as something that would run
            //database_exists = database.database_exists
        }

        public void set_recovery_mode(bool simple)
        {
            Log.bound_to(this).log_an_info_event_containing("Changing the database recovery mode if it has one to {0}", simple ? "simple" : "full");
        }

        public void backup_database(string output_path_minus_database)
        {
            Log.bound_to(this).log_an_info_event_containing("Backing up the database to \"{0}\".", output_path_minus_database);
        }

        public void restore_database(string restore_from_path, string custom_restore_options)
        {
            string message = "Mocking mode does NOT support what would happen under a restore circumstance, because it would have to actually restore the database to do so.";
            Log.bound_to(this).log_a_warning_event_containing(message);
            throw new ApplicationException(message);
        }

        public void delete_database_if_it_exists()
        {
            //TODO: Determine whether the database exists
            //database.delete_database_if_it_exists();
        }

        public void run_database_specific_tasks()
        {
            if (!database_exists)
            {
                //TODO: figure out whether we do this or not
                //database.run_database_specific_tasks();
            }
        }

        public void create_or_update_roundhouse_tables()
        {
            database.create_or_update_roundhouse_tables();
        }

        public void run_sql(string sql_to_run,ConnectionType connection_type)
        {
            Log.bound_to(this).log_an_info_event_containing("Running statemtent: {0}{1}", Environment.NewLine, sql_to_run);
            //database.run_sql(sql_to_run);
        }

        public void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            // database.insert_script_run(script_name, sql_to_run, sql_to_run_hash, run_this_script_once, version_id);
        }

        public void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string repository_version, string repository_path)
        {
            // database.insert_script_run_error(script_name, sql_to_run, sql_erroneous_part, error_message, repository_version, repository_path);
        }

        public string get_version(string repository_path)
        {
            if (database_exists)
            {
                return database.get_version(repository_path);
            }

            return string.Empty;
        }

        public long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            return 0;
        }

        public bool has_run_script_already(string script_name)
        {
            if (database_exists)
            {
                return database.has_run_script_already(script_name);
            }

            return false;
        }

        public string get_current_script_hash(string script_name)
        {
            if (database_exists)
            {
                return database.get_current_script_hash(script_name);
            }

            return string.Empty;
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