namespace roundhouse.databases
{
    using System;
    using roundhouse.infrastructure.app;
    using roundhouse.infrastructure.logging;

    public class DryRunDatabase : DatabaseDecoratorBase
    {
        private bool database_exists = false;

        public DryRunDatabase(Database database) : base(database)
        {
        }

        public override void open_connection(bool with_transaction)
        {
            if (database_exists)
            {
                database.open_connection(with_transaction);
            }
        }

        public override void close_connection()
        {
            if (database_exists)
            {
                database.close_connection();
            }
        }

        public override void open_admin_connection()
        {
            database.open_connection(false);
        }

        public override void close_admin_connection()
        {
            database.close_connection();
        }

        public override bool create_database_if_it_doesnt_exist(string custom_create_database_script)
        {
            database_exists = true; // Pretend that we've created database in mocking mode
            return true;
            //TODO: Don't allow creation of the database - record everything from here on out as something that would run
            //database_exists = database.database_exists
            //return database.
        }

        public override void set_recovery_mode(bool simple)
        {
            Log.bound_to(this).log_an_info_event_containing("Changing the database recovery mode if it has one to {0}", simple ? "simple" : "full");
        }

        public override void backup_database(string output_path_minus_database)
        {
            Log.bound_to(this).log_an_info_event_containing("Backing up the database to \"{0}\".", output_path_minus_database);
        }

        public override void restore_database(string restore_from_path, string custom_restore_options)
        {
            string message = "Mocking mode does NOT support what would happen under a restore circumstance, because it would have to actually restore the database to do so.";
            Log.bound_to(this).log_a_warning_event_containing(message);
            throw new ApplicationException(message);
        }

        public override void delete_database_if_it_exists()
        {
            //TODO: Determine whether the database exists
            //database.delete_database_if_it_exists();
        }

        public override void run_database_specific_tasks()
        {
            if (!database_exists)
            {
                //TODO: figure out whether we do this or not
                //database.run_database_specific_tasks();
            }
        }


        public override void run_sql(string sql_to_run, ConnectionType connection_type)
        {
            Log.bound_to(this).log_an_info_event_containing("Running statemtent: {0}{1}", Environment.NewLine, sql_to_run);
            //database.run_sql(sql_to_run);
        }

        public override object run_sql_scalar(string sql_to_run, ConnectionType connection_type)
        {
            Log.bound_to(this).log_an_info_event_containing("Running statemtent: {0}{1}", Environment.NewLine, sql_to_run);
            //database.run_sql(sql_to_run);
            return new object();
        }

        public override void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            // database.insert_script_run(script_name, sql_to_run, sql_to_run_hash, run_this_script_once, version_id);
        }

        public override void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string repository_version, string repository_path)
        {
            // database.insert_script_run_error(script_name, sql_to_run, sql_erroneous_part, error_message, repository_version, repository_path);
        }

        public override string get_version(string repository_path)
        {
            if (database_exists)
            {
                return database.get_version(repository_path);
            }

            return string.Empty;
        }

        public override long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            return 0;
        }

        public override bool has_run_script_already(string script_name)
        {
            if (database_exists)
            {
                return database.has_run_script_already(script_name);
            }

            return false;
        }

        public override string get_current_script_hash(string script_name)
        {
            if (database_exists)
            {
                return database.get_current_script_hash(script_name);
            }

            return string.Empty;
        }
    }
}