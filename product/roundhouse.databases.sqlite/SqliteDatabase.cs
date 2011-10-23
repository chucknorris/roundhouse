namespace roundhouse.databases.sqlite
{
    using System;
    using System.Data.SQLite;
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
        //            "This options could not be changed because Sqlite database migrator always splits batch statements by using SqliteScript class from Sqlite ADO.NET provider");
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

            if (string.IsNullOrEmpty(connection_string))
            {
                connection_string = build_connection_string(database_name);
            }

            configuration_property_holder.ConnectionString = connection_string;
            if (string.IsNullOrEmpty(admin_connection_string))
            {
                admin_connection_string = connection_string;
            }

            configuration_property_holder.ConnectionStringAdmin = admin_connection_string;

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

        public override string create_database_script()
        {
            return string.Empty;
            //return string.Format(
            //    @"CREATE DATABASE IF NOT EXISTS `{0}`;",
            //    database_name);
        }

        public override string delete_database_script()
        {
            throw new NotImplementedException("This is not yet implemented in RH/SQLite");
        }

        public override void run_database_specific_tasks()
        {
            Log.bound_to(this).log_a_debug_event_containing("Sqlite has no database specific tasks. Moving along now...");
        }

        //public override void run_sql(string sql_to_run, ConnectionType connection_type)
        //{
        //    if (string.IsNullOrEmpty(sql_to_run)) return;


        //    var connection = server_connection.underlying_type().downcast_to<SQLiteConnection>();
        //    //if (connection_type == ConnectionType.Admin)
        //    //{
        //    //    connection = admin_connection.underlying_type().downcast_to<SqliteConnection>();
        //    //}
        //    var command = new SQLiteCommand( sql_to_run,connection);
        //    command.CommandTimeout = 
        //    command.ExecuteNonQuery();
        //}

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