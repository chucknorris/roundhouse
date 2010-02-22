using System;
using System.Text.RegularExpressions;
using roundhouse.databases;
using roundhouse.infrastructure.logging;

namespace roundhouse.migrators
{
    using cryptography;
    using infrastructure.extensions;

    public sealed class DefaultDatabaseMigrator : DatabaseMigrator
    {
        public Database database { get; set; }
        private readonly CryptographicService crypto_provider;
        private readonly bool restoring_database;
        private readonly string restore_path;
        private readonly string custom_restore_options;
        private readonly string output_path;
        private readonly bool error_on_one_time_script_changes;
        private bool running_in_a_transaction = false;

        public DefaultDatabaseMigrator(Database database, CryptographicService crypto_provider, bool restoring_database, string restore_path, string custom_restore_options, string output_path, bool error_on_one_time_script_changes)
        {
            this.database = database;
            this.crypto_provider = crypto_provider;
            this.restoring_database = restoring_database;
            this.restore_path = restore_path;
            this.custom_restore_options = custom_restore_options;
            this.output_path = output_path;
            this.error_on_one_time_script_changes = error_on_one_time_script_changes;
        }

        public void connect(bool with_transaction)
        {
            running_in_a_transaction = with_transaction;
            database.initialize_connection();
            database.open_connection(with_transaction);
        }

        public void disconnect()
        {
            database.close_connection();
        }

        public void create_or_restore_database()
        {
            Log.bound_to(this).log_an_info_event_containing("Creating {0} database on {1} server if it doesn't exist.",
                                                             database.database_name, database.server_name);

            if (running_in_a_transaction)
            {
                database.close_connection();
                database.open_connection(false);
            }
            database.create_database_if_it_doesnt_exist();

            if (restoring_database)
            {
                restore_database(restore_path, custom_restore_options);
            }

            if (running_in_a_transaction)
            {
                database.close_connection();
                database.open_connection(true);
            }
        }

        public void backup_database_if_it_exists()
        {
            database.backup_database(output_path);
        }

        public void restore_database(string restore_from_path, string restore_options)
        {
            Log.bound_to(this).log_an_info_event_containing("Restoring {0} database on {1} server from path {2}.", database.database_name, database.server_name, restore_from_path);
            database.restore_database(restore_from_path, restore_options);
        }

        public void set_recovery_mode(bool simple)
        {
            if (running_in_a_transaction)
            {
                database.close_connection();
                database.open_connection(false);
            }
            database.set_recovery_mode(simple);

            if (running_in_a_transaction)
            {
                database.close_connection();
                database.open_connection(true);
            }
        }

        public void transfer_to_database_for_changes()
        {
            database.use_database(database.database_name);
        }

        public void verify_or_create_roundhouse_tables()
        {
            Log.bound_to(this).log_an_info_event_containing("Creating {0} schema if it doesn't exist.", database.roundhouse_schema_name);
            database.create_roundhouse_schema_if_it_doesnt_exist();
            Log.bound_to(this).log_an_info_event_containing("Creating [{0}].[{1}] table if it doesn't exist.", database.roundhouse_schema_name, database.version_table_name);
            database.create_roundhouse_version_table_if_it_doesnt_exist();
            Log.bound_to(this).log_an_info_event_containing("Creating [{0}].[{1}] table if it doesn't exist.", database.roundhouse_schema_name, database.scripts_run_table_name);
            database.create_roundhouse_scripts_run_table_if_it_doesnt_exist();
        }

        public string get_current_version(string repository_path)
        {
            string current_version = database.get_version(repository_path);

            if (string.IsNullOrEmpty(current_version))
            {
                current_version = "0";
            }

            return current_version;
        }

        public void delete_database()
        {
            Log.bound_to(this).log_an_info_event_containing("Deleting {0} database on {1} server if it exists.", database.database_name, database.server_name);

            if (running_in_a_transaction)
            {
                database.close_connection();
                database.open_connection(false);
            }

            database.delete_database_if_it_exists();

            if (running_in_a_transaction)
            {
                database.close_connection();
                database.open_connection(true);
            }
        }

        public long version_the_database(string repository_path, string repository_version)
        {
            Log.bound_to(this).log_an_info_event_containing("Versioning {0} database with version {1} based on {2}.",
                                                            database.database_name,
                                                            repository_version, repository_path);
            return database.insert_version_and_get_version_id(repository_path, repository_version);
        }

        public bool run_sql(string sql_to_run, string script_name, bool run_this_script_once, bool run_this_script_every_time, long version_id, environments.Environment environment)
        {
            bool this_sql_ran = false;

            if (this_is_a_one_time_script_that_has_changes_but_has_already_been_run(script_name, sql_to_run, run_this_script_once))
            {
                if (error_on_one_time_script_changes)
                {
                    throw new Exception(string.Format("{0} has changed since the last time it was run. By default this is not allowed - scripts that run once should never change. To change this behavior to a warning, please set warnOnOneTimeScriptChanges to true and run again. Stopping execution.", script_name));
                }
                Log.bound_to(this).log_a_warning_event_containing("{0} is a one time script that has changed since it was run.", script_name);
            }

            if (if_this_is_an_environment_file_its_in_the_right_environment(script_name, environment) && this_script_should_run(script_name, sql_to_run, run_this_script_once, run_this_script_every_time))
            {
                Log.bound_to(this).log_an_info_event_containing("Running {0} on {1} - {2}.", script_name, database.server_name, database.database_name);

                foreach (var sql_statement in Regex.Split(sql_to_run, database.sql_statement_separator_regex_pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
                {
                    if (script_has_text_to_run(sql_statement))
                    {
                        database.run_sql(sql_statement);
                    }
                }
                record_script_in_scripts_run_table(script_name, sql_to_run, run_this_script_once, version_id);
                this_sql_ran = true;
            }
            else
            {
                Log.bound_to(this).log_an_info_event_containing("Skipped {0} - {1}.", script_name, run_this_script_once ? "One time script" : "No changes were found to run");
            }

            return this_sql_ran;
        }

        private bool script_has_text_to_run(string sql_statement)
        {
            sql_statement = Regex.Replace(sql_statement, database.sql_statement_separator_regex_pattern, string.Empty, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            return !string.IsNullOrEmpty(sql_statement.to_lower().Replace(Environment.NewLine, string.Empty).Replace(" ", string.Empty));
        }

        public void record_script_in_scripts_run_table(string script_name, string sql_to_run, bool run_this_script_once, long version_id)
        {
            Log.bound_to(this).log_a_debug_event_containing("Recording {0} script ran on {1} - {2}.", script_name, database.server_name, database.database_name);
            database.insert_script_run(script_name, sql_to_run, create_hash(sql_to_run), run_this_script_once, version_id);
        }

        private string create_hash(string sql_to_run)
        {
            return crypto_provider.hash(sql_to_run.Replace(@"'", @"''"));
        }

        private bool this_script_has_run_already(string script_name)
        {
            return database.has_run_script_already(script_name);
        }

        private bool this_is_a_one_time_script_that_has_changes_but_has_already_been_run(string script_name, string sql_to_run, bool run_this_script_once)
        {
            return this_script_has_changed_since_last_run(script_name, sql_to_run) && this_script_has_run_already(script_name) && run_this_script_once;
        }

        private bool this_script_has_changed_since_last_run(string script_name, string sql_to_run)
        {
            bool hash_is_same = false;

            string old_text_hash = string.Empty;
            try
            {
                old_text_hash = database.get_current_script_hash(script_name);
            }
            catch (Exception)
            {
                Log.bound_to(this).log_an_info_event_containing("{0} - I didn't find this script executed before.", script_name);
            }

            if (string.IsNullOrEmpty(old_text_hash)) return true;

            string new_text_hash = create_hash(sql_to_run);

            if (string.Compare(old_text_hash, new_text_hash, true) == 0)
            {
                hash_is_same = true;
            }

            return !hash_is_same;
        }

        private bool this_script_should_run(string script_name, string sql_to_run, bool run_this_script_once, bool run_this_script_every_time)
        {
            if (run_this_script_every_time)
            {
                return true;
            }

            if (this_script_has_run_already(script_name) && run_this_script_once)
            {
                return false;
            }

            return this_script_has_changed_since_last_run(script_name, sql_to_run);
        }

        private bool if_this_is_an_environment_file_its_in_the_right_environment(string script_name, environments.Environment environment)
        {
            //Log.bound_to(this).log_an_info_event_containing("Checking to see if {0} is an environment file. We are in the {1} environment.", script_name, environment.name);
            if (!script_name.to_lower().Contains(".env."))
            {
                return true;
            }

            bool environment_file_is_in_the_right_environment = false;

            if (script_name.to_lower().Contains(environment.name.to_lower() + "."))
            {
                environment_file_is_in_the_right_environment = true;
            }

            Log.bound_to(this).log_an_info_event_containing("{0} is an environment file. We are in the {1} environment. This will{2} run based on this check.", script_name, environment.name, environment_file_is_in_the_right_environment ? string.Empty : " NOT");

            return environment_file_is_in_the_right_environment;
        }

    }
}