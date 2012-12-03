using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using roundhouse.infrastructure.app;

namespace roundhouse.databases.ravendb
{
    public class RavenDatabase : Database
    {
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

        public void open_connection(bool with_transaction)
        {
            throw new NotImplementedException();
        }

        public void close_connection()
        {
            throw new NotImplementedException();
        }

        public void open_admin_connection()
        {
            throw new NotImplementedException();
        }

        public void close_admin_connection()
        {
            throw new NotImplementedException();
        }

        public void rollback()
        {
            throw new NotImplementedException();
        }

        public bool create_database_if_it_doesnt_exist(string custom_create_database_script)
        {
            throw new NotImplementedException();
        }

        public void set_recovery_mode(bool simple)
        {
            throw new NotImplementedException();
        }

        public void backup_database(string output_path_minus_database)
        {
            throw new NotImplementedException();
        }

        public void restore_database(string restore_from_path, string custom_restore_options)
        {
            throw new NotImplementedException();
        }

        public void delete_database_if_it_exists()
        {
            throw new NotImplementedException();
        }

        public void run_database_specific_tasks()
        {
            throw new NotImplementedException();
        }

        public void create_or_update_roundhouse_tables()
        {
            throw new NotImplementedException();
        }

        public void run_sql(string sql_to_run, ConnectionType connection_type)
        {
            throw new NotImplementedException();
        }

        public object run_sql_scalar(string sql_to_run, ConnectionType connection_type)
        {
            throw new NotImplementedException();
        }

        public void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once,
                                      long version_id)
        {
            throw new NotImplementedException();
        }

        public void insert_script_run_error(string script_name, string sql_to_run, string sql_erroneous_part, string error_message,
                                            string repository_version, string repository_path)
        {
            throw new NotImplementedException();
        }

        public string get_version(string repository_path)
        {
            throw new NotImplementedException();
        }

        public long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            throw new NotImplementedException();
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
