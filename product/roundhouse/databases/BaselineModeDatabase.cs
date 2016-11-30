
namespace roundhouse.databases
{
    using System;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.app;

    class BaselineModeDatabase : DatabaseDecoratorBase
    {
        public BaselineModeDatabase(Database database)
            : base(database)
        {
        }

        private bool database_exists = false;

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

        public override bool create_database_if_it_doesnt_exist(string custom_create_database_script)
        {
            database_exists = true;
            return true;
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
        }

        public override object run_sql_scalar(string sql_to_run, ConnectionType connection_type)
        {
            Log.bound_to(this).log_an_info_event_containing("Running statemtent: {0}{1}", Environment.NewLine, sql_to_run);
            return new object();
        }

        public override string get_version(string repository_path)
        {
            if (database_exists)
            {
                return database.get_version(repository_path);
            }

            return string.Empty;
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