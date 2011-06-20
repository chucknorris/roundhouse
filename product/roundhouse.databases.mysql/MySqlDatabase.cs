namespace roundhouse.databases.mysql
{
    using System;
    using infrastructure.app;
    using infrastructure.extensions;
    using MySql.Data.MySqlClient;

    public class MySqlDatabase : AdoNetDatabase
    {
        private readonly MySqlAdoNetProviderResolver my_sql_ado_net_provider_resolver;

        public MySqlDatabase()
        {
            my_sql_ado_net_provider_resolver = new MySqlAdoNetProviderResolver();
            my_sql_ado_net_provider_resolver.register_db_provider_factory();
            my_sql_ado_net_provider_resolver.enable_loading_from_merged_assembly();
        }

        public override bool split_batch_statements
        {
            get { return false; }
            set
            {
                throw new Exception(
                    "This options could not be changed because MySQL database migrator always splits batch statements by using MySqlScript class from MySQL ADO.NET provider");
            }
        }

        public override void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (!string.IsNullOrEmpty(connection_string))
            {
                string[] parts = connection_string.Split(';');
                foreach (string part in parts)
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
                var builder = new MySqlConnectionStringBuilder(connection_string);
                builder.Database = "information_schema";
                admin_connection_string = builder.ConnectionString;
            }
        }

        public override void set_provider()
        {
            // http://stackoverflow.com/questions/1216626/how-to-use-ado-net-dbproviderfactory-with-mysql/1216887#1216887
            provider = "MySql.Data.MySqlClient";
        }

        public override string create_database_script()
        {
            return string.Format(
                @"CREATE DATABASE IF NOT EXISTS `{0}`;",
                database_name);
        }

        public override string delete_database_script()
        {
            return string.Format(
                @"DROP DATABASE IF EXISTS `{0}`;",
                database_name);
        }

        public override void run_database_specific_tasks()
        {
            //nothing to see here. move along now.
        }

        // http://bugs.mysql.com/bug.php?id=46429
        public override void run_sql(string sql_to_run)
        {
            if (string.IsNullOrEmpty(sql_to_run)) return;

            //TODO Investigate how pass CommandTimeout into commands which will be during MySqlScript execution.
            var connection = server_connection.underlying_type().downcast_to<MySqlConnection>();
            var script = new MySqlScript(connection, sql_to_run);
            script.Execute();
        }

        public override string set_recovery_mode_script(bool simple)
        {
            return string.Empty;
        }

        public override string restore_database_script(string restore_from_path, string custom_restore_options)
        {
            throw new NotImplementedException();
        }
    }
}