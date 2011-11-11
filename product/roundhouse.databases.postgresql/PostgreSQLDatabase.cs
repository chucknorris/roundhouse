namespace roundhouse.databases.postgresql
{
    using System;
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.logging;
    using Npgsql;

    public class PostgreSQLDatabase : AdoNetDatabase
    {
        private readonly PostgreAdoNetProviderResolver postgre_ado_net_provider_resolver;

        public PostgreSQLDatabase()
        {
            postgre_ado_net_provider_resolver = new PostgreAdoNetProviderResolver();
            postgre_ado_net_provider_resolver.register_db_provider_factory();
            postgre_ado_net_provider_resolver.enable_loading_from_merged_assembly();
        }

        public override void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (!string.IsNullOrEmpty(connection_string))
            {
                var parts = connection_string.Split(';');
                foreach (var part in parts)
                {
                    if (string.IsNullOrEmpty(server_name) && (part.to_lower().Contains("server") || part.to_lower().Contains("data source")))
                    {
                        server_name = part.Substring(part.IndexOf("=") + 1);
                    }

                    if (string.IsNullOrEmpty(database_name) && (part.to_lower().Contains("database") || part.to_lower().Contains("database")))
                    {
                        database_name = part.Substring(part.IndexOf("=") + 1);
                    }
                }
            }

            configuration_property_holder.ConnectionString = connection_string;

            set_provider();
            if (string.IsNullOrEmpty(admin_connection_string))
            {
                var builder = new NpgsqlConnection(connection_string);
                admin_connection_string = builder.ConnectionString;
            }
            configuration_property_holder.ConnectionStringAdmin = admin_connection_string;
        }

        public override void set_provider()
        {
            provider = "Npgsql";
        }

        public override string create_database_script()
        {
            //TODO: Add IF EXISTS condition to CREATE DATABASE
            return string.Format(
                @"CREATE DATABASE {0};",
                database_name);
        }

        public override string set_recovery_mode_script(bool simple)
        {
            return string.Empty;
        }

        public override string restore_database_script(string restore_from_path, string custom_restore_options)
        {
            throw new NotImplementedException();
        }

        public override string delete_database_script()
        {
            //TODO: Add IF EXISTS condition to DROP DATABASE
            return string.Format(
                @"DROP DATABASE {0};",
                database_name);
        }

        public override void create_or_update_roundhouse_tables()
        {
            Log.bound_to(this).log_an_info_event_containing("Creating schema [{0}].", roundhouse_schema_name);
            run_sql(TableCreationScripts.create_roundhouse_schema(roundhouse_schema_name), ConnectionType.Default);
            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].", version_table_name);
            run_sql(TableCreationScripts.create_roundhouse_version_table(roundhouse_schema_name, version_table_name), ConnectionType.Default);
            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].", scripts_run_table_name);
            run_sql(TableCreationScripts.create_roundhouse_scripts_run_table(roundhouse_schema_name, version_table_name, scripts_run_table_name),
                    ConnectionType.Default);
            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}].", scripts_run_errors_table_name);
            run_sql(TableCreationScripts.create_roundhouse_scripts_run_errors_table(roundhouse_schema_name, scripts_run_errors_table_name),
                    ConnectionType.Default);
        }

        public override void run_database_specific_tasks()
        {
            Log.bound_to(this).log_a_debug_event_containing("PostgreSQL has no database specific tasks. Moving along now...");
        }
    }
}