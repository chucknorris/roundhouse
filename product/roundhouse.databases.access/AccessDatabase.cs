using roundhouse.infrastructure.logging;

namespace roundhouse.databases.access
{
    using System;
    using System.Data.OleDb;
    using connections;
    using infrastructure.app;
    using infrastructure.extensions;

    public class AccessDatabase : AdoNetDatabase
    {
        private string connect_options = "Trusted_Connection";

        public override bool supports_ddl_transactions
        {
            get { return false; }
        }

        public override void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (!string.IsNullOrEmpty(connection_string))
            {
                string[] parts = connection_string.Split(';');
                foreach (string part in parts)
                {
                    if (string.IsNullOrEmpty(server_name) && part.to_lower().Contains("server"))
                    {
                        server_name = part.Substring(part.IndexOf("=") + 1);
                    }

                    if (string.IsNullOrEmpty(database_name) && part.to_lower().Contains("database"))
                    {
                        database_name = part.Substring(part.IndexOf("=") + 1);
                    }
                }

                if (!connection_string.to_lower().Contains(connect_options.to_lower()))
                {
                    connect_options = string.Empty;
                    foreach (string part in parts)
                    {
                        if (!part.to_lower().Contains("server") && !part.to_lower().Contains("data source") && !part.to_lower().Contains("initial catalog") &&
                            !part.to_lower().Contains("database"))
                        {
                            connect_options += part + ";";
                        }
                    }
                }
            }

            if (connect_options == "Trusted_Connection")
            {
                connect_options = "Trusted_Connection=yes;";
            }

            if (string.IsNullOrEmpty(connection_string))
            {
                connection_string = build_connection_string(server_name, database_name, connect_options);
            }

            if (connection_string.to_lower().Contains("sqlserver") || connection_string.to_lower().Contains("sqlncli"))
            {
                connection_string = build_connection_string(server_name, database_name, connect_options);
            }
            configuration_property_holder.ConnectionString = connection_string;

            set_provider();
            admin_connection_string = connection_string;
            configuration_property_holder.ConnectionStringAdmin = admin_connection_string;
            //set_repository(configuration_property_holder);
        }

        public override void open_admin_connection()
        {
            admin_connection = new AdoNetConnection(new OleDbConnection(admin_connection_string));
            admin_connection.open();
        }

        public override void open_connection(bool with_transaction)
        {
            server_connection = new AdoNetConnection(new OleDbConnection(connection_string));
            server_connection.open();

            set_repository();

            if (with_transaction)
            {
                transaction = server_connection.underlying_type().BeginTransaction();
                repository.start(true);
            }
        }

        public override void set_provider()
        {
            provider = ((OleDbConnection) server_connection.underlying_type()).Provider;
        }

        private static string build_connection_string(string server_name, string database_name, string connection_options)
        {
            return string.Format("Provider=SQLNCLI;Server={0};Database={1};{2}", server_name, database_name, connection_options);
        }

        public override void run_database_specific_tasks()
        {
            Log.bound_to(this).log_a_debug_event_containing("Access has no specific database tasks. Returning...");
            //TODO: Anything for Access?
        }

        public override string create_database_script()
        {
            return string.Empty;
        }

        public override string set_recovery_mode_script(bool simple)
        {
            return string.Empty;
        }

        public override string restore_database_script(string restore_from_path, string custom_restore_options)
        {
            return string.Empty;
        }

        public override string delete_database_script()
        {
            return string.Empty;
        }
    }
}