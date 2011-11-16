using System.Text.RegularExpressions;
using roundhouse.consoles;

namespace roundhouse.databases.postgresql
{
    using System;
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.logging;
    using Npgsql;

    public class PostgreSQLDatabase : AdoNetDatabase
    {
        public override bool split_batch_statements
        {
            get { return false; }
            set
            {

                throw new Exception(
                    "This option can not be changed because PostgreSQL database migrator always splits batch statements by using Npgsql class from Npgsql ADO.NET provider");
            }
        }

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

            server_name = server_name.to_lower();
            database_name = database_name.to_lower();
            configuration_property_holder.ServerName = server_name;
            configuration_property_holder.DatabaseName = database_name;


            if (server_name == infrastructure.ApplicationParameters.default_server_name)
            {
                server_name = "localhost";
            }

            if (string.IsNullOrEmpty(connection_string))
            {
                InteractivePrompt.write_header(configuration_property_holder);
                var user_name = InteractivePrompt.get_user("postgres", configuration_property_holder);
                var password = InteractivePrompt.get_password("postgres", configuration_property_holder);
                InteractivePrompt.write_footer();

                connection_string = build_connection_string(server_name, database_name, user_name, password);
            }

            configuration_property_holder.ConnectionString = connection_string;

            set_provider();
            if (string.IsNullOrEmpty(admin_connection_string))
            {
                admin_connection_string = Regex.Replace(connection_string, "database=.*?;", "database=postgres;", RegexOptions.IgnoreCase);
            }
            configuration_property_holder.ConnectionStringAdmin = admin_connection_string;
        }

        public override void set_provider()
        {
            provider = "Npgsql";
        }

        private static string build_connection_string(string server_name, string database_name, string user_name, string password)
        {
            return string.Format("Server={0};Database={1};Port=5432;UserId={2};Password={3};", server_name, database_name, user_name, password);
        }

        public override string create_database_script()
        {
            return string.Format("CREATE DATABASE {0};", database_name);

            //            return string.Format(
            //                @"
            //--CREATE FUNCTION RH_CreateDb() RETURNS void AS $$
            //--DECLARE 
            //--    t_exists integer;
            //--    --t_created boolean;
            //--BEGIN
            //--    --set t_created = false;
            //--    select INTO t_exists count(*) from pg_catalog.pg_database where datname = '{0}';
            //--	IF t_exists = 0 THEN
            //		CREATE DATABASE {0};
            //--        --t_created = true;
            //--	END IF;	
            //
            //--    --return t_created;
            //--END;
            //--$$ LANGUAGE 'plpgsql';
            //--SELECT RH_CreateDb();
            //--DROP FUNCTION RH_CreateDb();
            //",
            //                database_name);
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
            //compatible starting at 8.2
            return string.Format(@"DROP DATABASE IF EXISTS {0};", database_name);
        }

        public override void create_or_update_roundhouse_tables()
        {
            //Log.bound_to(this).log_an_info_event_containing("Creating schema [{0}].", roundhouse_schema_name);
            //run_sql(TableCreationScripts.create_roundhouse_schema(roundhouse_schema_name), ConnectionType.Default);
            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}_{1}].", roundhouse_schema_name, version_table_name);
            run_sql(TableCreationScripts.create_roundhouse_version_table(roundhouse_schema_name, version_table_name), ConnectionType.Default);
            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}_{1}].", roundhouse_schema_name, scripts_run_table_name);
            run_sql(TableCreationScripts.create_roundhouse_scripts_run_table(roundhouse_schema_name, version_table_name, scripts_run_table_name),
                    ConnectionType.Default);
            Log.bound_to(this).log_an_info_event_containing("Creating table [{0}_{1}].", roundhouse_schema_name, scripts_run_errors_table_name);
            run_sql(TableCreationScripts.create_roundhouse_scripts_run_errors_table(roundhouse_schema_name, scripts_run_errors_table_name),
                    ConnectionType.Default);
        }

        public override void run_database_specific_tasks()
        {
            Log.bound_to(this).log_a_debug_event_containing("PostgreSQL has no database specific tasks. Moving along now...");
        }
    }
}