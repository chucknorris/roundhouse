namespace roundhouse.sql
{
    public interface SqlScript
    {
        string separator_characters_regex { get; }
        string create_database(string database_name);
        string set_recovery_mode(string database_name, bool simple);
        string restore_database(string database_name, string restore_from_path, string custom_restore_options);
        string delete_database(string database_name);
        //roundhouse specific 
        string create_roundhouse_schema(string roundhouse_schema_name);
        string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name);

        string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name,
                                                   string scripts_run_table_name);
        //functions
        string use_database(string database_name);
        string get_version(string roundhouse_schema_name, string version_table_name, string repository_path);
        string insert_version(string roundhouse_schema_name, string version_table_name,
                                                 string repository_path, string repository_version, string user_name);
        string get_version_id(string roundhouse_schema_name, string version_table_name, string repository_path);
        string get_current_script_hash(string roundhouse_schema_name, string scripts_run_table_name, string script_name);
        string has_script_run(string roundhouse_schema_name, string scripts_run_table_name, string script_name);
        string insert_script_run(string roundhouse_schema_name, string scripts_run_table_name, long version_id,
                                 string script_name, string sql_to_run, string sql_to_run_hash,
                                 bool run_this_script_once, string user_name);
    }
}