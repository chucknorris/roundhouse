namespace roundhouse.databases.postgresql
{
    using infrastructure.app;
    using infrastructure.logging;
    using Npgsql;

    public class RedshiftSQLDatabase : PostgreSQLDatabase
    {
        public override string create_database_script()
        {
            var check_database_exists_script = $@"
SELECT 
count (*)
FROM pg_catalog.pg_database
WHERE datname = lower('{database_name}');";

            var db_exists = (long) run_sql_scalar(check_database_exists_script, ConnectionType.Default);
            return db_exists == 0 
                ? $"CREATE DATABASE {database_name};" 
                : "SELECT 0";
        }

        protected override string build_connection_string(string host, string database, string username, string password)
        {
            var cs = base.build_connection_string(host, database, username, password);
            
            var csb = new NpgsqlConnectionStringBuilder(cs);
            csb.ServerCompatibilityMode = ServerCompatibilityMode.Redshift;

            return csb.ToString();
        }

        public override void create_or_update_roundhouse_tables()
        {
            Log.bound_to(this).log_an_info_event_containing("Creating schema [{0}].", roundhouse_schema_name);
            var schema_exists = (long)run_sql_scalar(RedshiftTableCreationScripts.select_exists_roundhouse_schema(roundhouse_schema_name), ConnectionType.Default);
            if(schema_exists == 0) run_sql(RedshiftTableCreationScripts.create_roundhouse_schema(roundhouse_schema_name), ConnectionType.Default);

            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].[{1}].", roundhouse_schema_name, version_table_name);
            var version_table_exists = (long)run_sql_scalar(RedshiftTableCreationScripts.select_exists_roundhouse_version_table(roundhouse_schema_name, version_table_name), ConnectionType.Default);
            if(version_table_exists == 0) run_sql(RedshiftTableCreationScripts.create_roundhouse_version_table(roundhouse_schema_name, version_table_name), ConnectionType.Default);

            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].[{1}].", roundhouse_schema_name, scripts_run_table_name);
            var scripts_run_table_exists = (long)run_sql_scalar(RedshiftTableCreationScripts.select_exists_roundhouse_scripts_run_table(roundhouse_schema_name, scripts_run_table_name), ConnectionType.Default);
            if(scripts_run_table_exists == 0) run_sql(RedshiftTableCreationScripts.create_roundhouse_scripts_run_table(roundhouse_schema_name, version_table_name, scripts_run_table_name), ConnectionType.Default);

            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].[{1}].", roundhouse_schema_name, scripts_run_errors_table_name);
            var scripts_run_errors_table_exists = (long)run_sql_scalar(RedshiftTableCreationScripts.select_exists_roundhouse_scripts_run_errors_table(roundhouse_schema_name, scripts_run_errors_table_name), ConnectionType.Default);
            if(scripts_run_errors_table_exists == 0) run_sql(RedshiftTableCreationScripts.create_roundhouse_scripts_run_errors_table(roundhouse_schema_name, scripts_run_errors_table_name), ConnectionType.Default);
        }
    }
}