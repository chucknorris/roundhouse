namespace roundhouse.databases
{
    using System;

    public interface Database : IDisposable
    {
        string server_name { get; set; }
        string database_name { get; set; }
        string provider { get; set; }
        string connection_string { get; set; }
        string roundhouse_schema_name { get; set; }
        string version_table_name { get; set; }
        string scripts_run_table_name { get; set; }
        string user_name { get; set; }
        string sql_statement_separator_regex_pattern { get;}
        string custom_create_database_script { get; set; }
        int restore_timeout { get; set; }


        void initialize_connection();
        void open_connection(bool with_transaction);
        void close_connection();

        void create_database_if_it_doesnt_exist();
        void set_recovery_mode(bool simple);
        void backup_database(string output_path_minus_database);
        void restore_database(string restore_from_path, string custom_restore_options);
        void delete_database_if_it_exists();
        void use_database(string database_name);
        void create_roundhouse_schema_if_it_doesnt_exist();
        void create_roundhouse_version_table_if_it_doesnt_exist();
        void create_roundhouse_scripts_run_table_if_it_doesnt_exist();
        void run_sql(string sql_to_run);
        void insert_script_run(string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, long version_id);
        
        string get_version(string repository_path);
        long insert_version_and_get_version_id(string repository_path, string repository_version);
        bool has_run_script_already(string script_name);
        string get_current_script_hash(string script_name);
        object run_sql_scalar(string sql_to_run);
    }
}