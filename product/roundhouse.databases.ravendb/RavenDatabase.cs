using System;
using System.Linq;
using roundhouse.databases.ravendb.commands;
using roundhouse.databases.ravendb.models;
using roundhouse.databases.ravendb.serializers;
using roundhouse.infrastructure.app;
using roundhouse.model;

namespace roundhouse.databases.ravendb
{
    public class RavenDatabase : Database
    {
        public RavenDatabase()
        {
            RavenCommandFactory = new RavenCommandFactory();
            Serializer = new JsonSerializer();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public IRavenCommandFactory RavenCommandFactory { get; set; }
        public ISerializer Serializer { get; set; }

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
            
        }

        public void backup_database(string output_path_minus_database)
        {
            
        }

        public void restore_database(string restore_from_path, string custom_restore_options)
        {
            
        }

        public void delete_database_if_it_exists()
        {
            
        }

        public void run_database_specific_tasks()
        {
            
        }

        public void create_or_update_roundhouse_tables()
        {
            
        }
        
        //TODO REPLACE ConnectionString
        public void run_sql(string sql_to_run, ConnectionType connection_type)
        {
            using (IRavenCommand command = RavenCommandFactory.CreateRavenCommand(sql_to_run))
            {
                command.ExecuteCommand();
            }
        }

        public object run_sql_scalar(string sql_to_run, ConnectionType connection_type)
        {
            throw new NotImplementedException();
        }

        public void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once,
                                      long version_id)
        {
            var scriptSuccessModel = new ScriptsRun
            {
                script_name = script_name,
                text_of_script = sql_to_run,
                text_hash = sql_to_run_hash,
                one_time_script = run_this_script_once,
                version_id = version_id,
                entered_by = user_name??"System"
            };
            var data = Serializer.SerializeObject(scriptSuccessModel);

            var scriptToRun = string.Format(@"PUT {0}/docs/ScriptsRun/{1} -d ""{2}"" ", connection_string, script_name, data);
            using (IRavenCommand command = RavenCommandFactory.CreateRavenCommand(scriptToRun))
            {
                command.ExecuteCommand();
            }
        }

        public void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message,
                                            string repository_version, string repository_path)
        {
            var scriptsRunError = new ScriptsRunError()
            {
                script_name = script_name,
                text_of_script = sql_to_run,
                error_message = error_message,
                repository_path = repository_path,
                version = repository_version,
                erroneous_part_of_script = sql_erroneous_part,
                entered_by = user_name??"System",
            };
            var data = Serializer.SerializeObject(scriptsRunError);

            var scriptToRun = string.Format(@"PUT {0}/docs/ScriptsRunError/{1} -d ""{2}"" ", connection_string, script_name, data);
            using (IRavenCommand command = RavenCommandFactory.CreateRavenCommand(scriptToRun))
            {
                command.ExecuteCommand();
            }
        }

        public string get_version(string repository_path)
        {
            //do a get of the versionfile - deserialize it and sort with linq on lowest
            var scriptToRun = string.Format(@"GET {0}/docs/Version", connection_string);
            string data;
            using (var command = RavenCommandFactory.CreateRavenCommand(scriptToRun))
            {
                data = command.ExecuteCommand();
            }
            var model = Serializer.DeserializeObject<VersionDocument>(data);

            var latestVersion=model.Versions.Where(s => s.RepositoryPath == repository_path)
                 .OrderByDescending(s => s.ModifiedDate)
                 .FirstOrDefault();
            if (latestVersion != null)
            {
                return latestVersion.Version;
            }
            return null;
        }

        public long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            return -1;
        }

        public bool has_run_script_already(string script_name)
        {
            throw new NotImplementedException();
        }

        public string get_current_script_hash(string script_name)
        {
            throw new NotImplementedException();
        }
    }
}
