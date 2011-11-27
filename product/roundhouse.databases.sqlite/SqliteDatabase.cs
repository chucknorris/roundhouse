namespace roundhouse.databases.sqlite
{
    using System;
    using System.Data.SQLite;
    using System.IO;
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.logging;

    public class SqliteDatabase : AdoNetDatabase
    {
        private readonly SqliteAdoNetProviderResolver sqlite_ado_net_provider_resolver;

        public SqliteDatabase()
        {
            sqlite_ado_net_provider_resolver = new SqliteAdoNetProviderResolver();
            sqlite_ado_net_provider_resolver.register_db_provider_factory();
            sqlite_ado_net_provider_resolver.enable_loading_from_merged_assembly();
        }

        //todo: sqlite - does splitting batch statements apply to sqlite?
        //public override bool split_batch_statements
        //{
        //    get { return false; }
        //    set
        //    {
        //        throw new Exception(
        //            "This option can not be changed because SQLite database migrator always splits batch statements by using SQLiteScript class from SQLite ADO.NET provider");
        //    }
        //}

        public override void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
        {
            server_name = "sqlite";
            if (!string.IsNullOrEmpty(connection_string))
            {
                string[] parts = connection_string.Split(';');
                foreach (string part in parts)
                {
                    if (string.IsNullOrEmpty(server_name) && part.to_lower().Contains("data source"))
                    {
                        database_name = part.Substring(part.IndexOf("=") + 1);
                    }
                }
            }

            if (database_name.to_lower() != ":memory:" && !database_name.to_lower().EndsWith(".db3"))
            {
                database_name = database_name + ".db3";
            }

            if (string.IsNullOrEmpty(connection_string))
            {
                connection_string = build_connection_string(database_name);
            }

            configuration_property_holder.ConnectionString = connection_string;
            if (string.IsNullOrEmpty(admin_connection_string))
            {
                admin_connection_string = connection_string;
            }

            configuration_property_holder.DatabaseName = database_name;
            configuration_property_holder.ConnectionStringAdmin = admin_connection_string;

            sqlite_ado_net_provider_resolver.output_path = configuration_property_holder.OutputPath;

            set_provider();
        }

        public override void set_provider()
        {
            provider = "System.Data.SQLite";
        }

        private static string build_connection_string(string database_name)
        {
            return string.Format("Data Source={0};Version=3;New=True;", database_name);
        }

        public override bool create_database_if_it_doesnt_exist(string custom_create_database_script)
        {
            if (!string.IsNullOrEmpty(custom_create_database_script)) throw new ApplicationException("You cannot specify a custom create database script with SQLite.");
            string db_file = create_database_script();

            //in memory, so we are creating
            if (string.IsNullOrEmpty(db_file)) return true;
            if (File.Exists(db_file)) return false;
            
            SQLiteConnection.CreateFile(db_file);

            return true;
        }

        public override string create_database_script()
        {
            if (database_name.to_lower() != ":memory:") return database_name;
            return string.Empty;
        }

        public override void delete_database_if_it_exists()
        {
            string db_file = delete_database_script();
            if (string.IsNullOrEmpty(db_file)) return;

            close_admin_connection();
            close_connection();

            if (File.Exists(db_file)) File.Delete(db_file);
        }

        public override string delete_database_script()
        {
            if (database_name.to_lower() != ":memory:") return database_name;
            return string.Empty;
        }

        public override void run_database_specific_tasks()
        {
            Log.bound_to(this).log_a_debug_event_containing("Sqlite has no database specific tasks. Moving along now...");
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