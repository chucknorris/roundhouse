using System;
using System.Security.Principal;
using roundhouse.databases.ravendb.commands;
using roundhouse.databases.ravendb.model;
using roundhouse.databases.ravendb.serializers;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.logging;
using roundhouse.model;
using Version = roundhouse.model.Version;

namespace roundhouse.databases.ravendb
{
    public class RavenDatabase : Database
    {
        public RavenDatabase()
        {
            Serializer = new JsonSerializer();
        }

        public void Dispose()
        {
        }

        public ConfigurationPropertyHolder configuration { get; set; }
        public string server_name { get; set; }
        public string database_name { get; set; }
        public string provider { get; set; }
        public string connection_string { get; set; }
        public string admin_connection_string { get; set; }
        public string roundhouse_schema_name { get; set; }
        public string version_table_name { get; set; }
        public string scripts_run_table_name { get; set; }
        public string scripts_run_errors_table_name { get; set; }
        public string user_name { get; set; }
        public string sql_statement_separator_regex_pattern { get; private set; }
        public int command_timeout { get; set; }
        public int admin_command_timeout { get; set; }
        public int restore_timeout { get; set; }
        public bool split_batch_statements { get; set; }
        public bool supports_ddl_transactions { get; private set; }

        public void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
        {

        }

        public ISerializer Serializer { get; set; }

        public string get_identity()
        {
            string identity_of_updater = WindowsIdentity.GetCurrent() != null ? WindowsIdentity.GetCurrent().Name : string.Empty;

            return identity_of_updater;
        }

        public void open_connection(bool with_transaction)
        {
        }

        public void close_connection()
        {
        }

        public void open_admin_connection()
        {

        }

        public void close_admin_connection()
        {
        }

        public void rollback()
        {
        }

        public bool create_database_if_it_doesnt_exist(string custom_create_database_script)
        {
            //create default if this doesn't exist
            run_sql(custom_create_database_script, ConnectionType.Admin);
            return true;
        }

        public void set_recovery_mode(bool simple)
        {
            Log.bound_to(this).log_a_warning_event_containing("{0} with provider {1} does not provide a facility for setting recovery mode to simple at this time.",
                                                              GetType(), provider);
        }

        public void backup_database(string output_path_minus_database)
        {
            Log.bound_to(this).log_a_warning_event_containing("{0} with provider {1} does not provide a facility for backing up a database at this time.",
                                                              GetType(), provider);
        }

        public void restore_database(string restore_from_path, string custom_restore_options)
        {
            Log.bound_to(this).log_a_warning_event_containing("{0} with provider {1} does not provide a facility for restoring a database at this time.",
                                                              GetType(), provider);
        }

        public void delete_database_if_it_exists()
        {
            Log.bound_to(this).log_an_error_event_containing("{0} with provider {1} does not provide a facility for deleting a database at this time.",
                                                             GetType(), provider);
        }

        public void run_database_specific_tasks()
        {
        }

        public void create_or_update_roundhouse_tables()
        {
        }

        public void run_sql(string sql_to_run, ConnectionType connection_type)
        {
            if (string.IsNullOrEmpty(sql_to_run)) return;

            //really naive retry logic. Consider Lokad retry policy
            //this is due to sql server holding onto a connection http://social.msdn.microsoft.com/Forums/en-US/adodotnetdataproviders/thread/99963999-a59b-4614-a1b9-869c6dff921e
            try
            {
                run_command_with(sql_to_run, connection_type);
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_a_debug_event_containing("Failure executing command, trying again. {0}{1}", Environment.NewLine, ex.ToString());
                run_command_with(sql_to_run, connection_type);
            }
        }

        private void run_command_with(string sql_to_run, ConnectionType connection_type)
        {
            using (IRavenCommand command = setup_database_command(sql_to_run, connection_type))
            {
                command.Execute();
                command.Dispose();
            }
        }

        public object run_sql_scalar(string sql_to_run, ConnectionType connection_type)
        {
            object return_value = new object();

            if (string.IsNullOrEmpty(sql_to_run)) return return_value;

            using (IRavenCommand command = setup_database_command(sql_to_run, connection_type))
            {
                return_value = command.Execute();
                command.Dispose();
            }

            return return_value;
        }

        private IRavenCommand setup_database_command(string script_to_run, ConnectionType connection_type)
        {
            IRavenCommand command = null;

            switch (connection_type)
            {
                case ConnectionType.Default:
                    Log.bound_to(this).log_a_debug_event_containing("Setting up command for normal connection");
                    command = RavenCommand.CreateCommand(connection_string, script_to_run);
                    command.CommandTimeout = command_timeout;
                    break;
                case ConnectionType.Admin:
                    Log.bound_to(this).log_a_debug_event_containing("Setting up command for admin connection");
                    command = RavenCommand.CreateCommand(admin_connection_string, script_to_run);
                    command.CommandTimeout = admin_command_timeout;
                    break;
            }

            return command;
        }

        public void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id)
        {
            var script_run = new ScriptsRun
                {
                    version_id = version_id,
                    script_name = script_name,
                    text_of_script = sql_to_run,
                    text_hash = sql_to_run_hash,
                    one_time_script = run_this_script_once,
                    entry_date = DateTime.Now,
                    modified_date = DateTime.Now,
                    entered_by = get_identity()
                };

            try
            {

                var address = string.Format("/docs/RoundhousE/ScriptsRun/{0}", script_name);
                var data = Serializer.SerializeObject(script_run);

                // put the document as new root (always last run)
                using (IRavenCommand command = RavenCommand.CreateCommand(connection_string, address, "PUT", null, data))
                {
                    command.Execute();
                }

                // put the document with version (history of runs)
                using (IRavenCommand command = RavenCommand.CreateCommand(connection_string, string.Format("{0}/{1}", address, version_id), "PUT", null, data))
                {
                    command.Execute();
                }
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_an_error_event_containing(
                    "{0} with provider {1} does not provide a facility for recording scripts run at this time.{2}{3}",
                    GetType(), provider, Environment.NewLine, ex.Message);
                throw;
            }
        }

        public void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message,
                                            string repository_version, string repository_path)
        {
            var script_run_error = new ScriptsRunError
                {
                    version = repository_version ?? string.Empty,
                    script_name = script_name,
                    text_of_script = sql_to_run,
                    erroneous_part_of_script = sql_erroneous_part,
                    repository_path = repository_path ?? string.Empty,
                    error_message = error_message,
                    entry_date = DateTime.Now,
                    modified_date = DateTime.Now,
                    entered_by = get_identity()
                };

            try
            {
                var address = string.Format("/docs/RoundhousE/ScriptsRunError/{0}/", script_name);

                using (IRavenCommand command = RavenCommand.CreateCommand(connection_string, address, "PUT", null, Serializer.SerializeObject(script_run_error)))
                {
                    command.Execute();
                }
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_an_error_event_containing(
                    "{0} with provider {1} does not provide a facility for recording scripts run errors at this time.{2}{3}",
                    GetType(), provider, Environment.NewLine, ex.Message);
                throw;
            }
        }

        public string get_version(string repository_path)
        {
            var versions = get_versions();
            return versions.GetLastVersionNumber(repository_path);
        }

        private Versions get_versions()
        {
            string versionsJson;

            using (IRavenCommand command = RavenCommand.CreateCommand(connection_string, "/docs/RoundhousE/Versions", "GET", null, null))
            {
                versionsJson = (string) command.Execute();
            }

            return versionsJson == null ?
                       new Versions() :
                       Serializer.DeserializeObject<Versions>(versionsJson);
        }

        private void set_versions(Versions versions)
        {
            using (IRavenCommand command = RavenCommand.CreateCommand(connection_string, "/docs/RoundhousE/Versions", "PUT", null, Serializer.SerializeObject(versions)))
            {
                command.Execute();
            }
        }

        public long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            var version = new Version
                {
                    version = repository_version ?? string.Empty,
                    repository_path = repository_path ?? string.Empty,
                    entry_date = DateTime.Now,
                    modified_date = DateTime.Now,
                    entered_by = get_identity()
                };

            try
            {
                var versions = get_versions();

                versions.AddVersionItem(version);

                set_versions(versions);

                return version.id;
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_an_error_event_containing("{0} with provider {1} does not provide a facility for inserting versions at this time.{2}{3}",
                                                                 GetType(), provider, Environment.NewLine, ex.Message);
                throw;
            }
        }

        public string get_current_script_hash(string script_name)
        {
            string hash = string.Empty;

            try
            {
                var address = string.Format("/docs/RoundhousE/ScriptsRun/{0}", script_name);
                string scriptsRunJson = null;

                using (IRavenCommand command = RavenCommand.CreateCommand(connection_string, address, "GET", null, null))
                {
                    scriptsRunJson = (string) command.Execute();
                }

                if (scriptsRunJson != null)
                {
                    hash = Serializer.DeserializeObject<ScriptsRun>(scriptsRunJson).text_hash;
                }
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_an_error_event_containing(
                    "{0} with provider {1} does not provide a facility for hashing (through recording scripts run) at this time.{2}{3}",
                    GetType(), provider, Environment.NewLine, ex.Message);
                throw;
            }

            return hash;
        }

        public bool has_run_script_already(string script_name)
        {
            bool script_has_run = false;

            try
            {
                var address = string.Format("/docs/RoundhousE/ScriptsRun/{0}", script_name);

                // todo: ?metadata-only=true
                using (IRavenCommand command = RavenCommand.CreateCommand(connection_string, address, "GET", null, null))
                {
                    script_has_run = command.Execute() != null;
                }
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_an_error_event_containing(
                    "{0} with provider {1} does not provide a facility for determining if a script has run at this time.{2}{3}",
                    GetType(), provider, Environment.NewLine, ex.Message);
                throw;
            }

            return script_has_run;
        }
    }
}
