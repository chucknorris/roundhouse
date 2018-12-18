using System.Data.Common;
using roundhouse.consoles;
using System.IO;

namespace roundhouse.databases.mysql
{
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.logging;
    using MySql.Data.MySqlClient;
    using System.Collections.Generic;
    using System;
    using System.IO;
    using System.Globalization;
    using System.Text;
    using roundhouse.databases.mysql.parser;

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
                    "This option can not be changed because MySQL database migrator always splits batch statements by using MySqlScript class from MySQL ADO.NET provider");
            }
        }

        protected override DbProviderFactory get_db_provider_factory()
        {
            return new MySqlClientFactory();
        }

        public override void initialize_connections(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (!string.IsNullOrEmpty(connection_string))
            {
                // Must allow User variables for SPROCs
                var connectionBuilder = new MySqlConnectionStringBuilder(connection_string);
                if (!connectionBuilder.AllowUserVariables)
                {
                    connectionBuilder.Add("Allow User Variables", "True");
                    connection_string = connectionBuilder.ConnectionString;
                }

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

            if (server_name == infrastructure.ApplicationParameters.default_server_name)
            {
                server_name = "localhost";
            }

            if (string.IsNullOrEmpty(connection_string))
            {
                InteractivePrompt.write_header(configuration_property_holder);
                var user_name = InteractivePrompt.get_user("root", configuration_property_holder);
                var password = InteractivePrompt.get_password("root", configuration_property_holder);
                InteractivePrompt.write_footer();

                connection_string = build_connection_string(server_name, database_name, user_name, password);
            }

            configuration_property_holder.ConnectionString = connection_string;

            set_provider();
            if (string.IsNullOrEmpty(admin_connection_string))
            {
                var builder = new MySqlConnectionStringBuilder(connection_string);
                builder.Database = "information_schema";
                admin_connection_string = builder.ConnectionString;
            }
            configuration_property_holder.ServerName = server_name;
            configuration_property_holder.DatabaseName = database_name;
            configuration_property_holder.ConnectionStringAdmin = admin_connection_string;
        }

        public override void set_provider()
        {
            // http://stackoverflow.com/questions/1216626/how-to-use-ado-net-dbproviderfactory-with-mysql/1216887#1216887
            provider = "MySql.Data.MySqlClient";
        }

        private static string build_connection_string(string server_name, string database_name, string user_name, string password)
        {
            var connectionBuilder = new MySqlConnectionStringBuilder();
            connectionBuilder.Server = server_name;
            connectionBuilder.Database = database_name;
            connectionBuilder.Port = 3306;
            connectionBuilder.UserID = user_name;
            connectionBuilder.Password = password;
            connectionBuilder.AllowUserVariables = true; // Must allow User variables for SPROCs

            return connectionBuilder.ConnectionString;
        }

        public override string create_database_script()
        {
            return string.Format(
                @"SET @Created = 1;

                SELECT 0 FROM INFORMATION_SCHEMA.SCHEMATA
                WHERE SCHEMA_NAME='{0}' INTO @Created;

                CREATE DATABASE IF NOT EXISTS `{0}`;

                SELECT @Created;",
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
            Log.bound_to(this).log_a_debug_event_containing("MySQL has no database specific tasks. Moving along now...");
        }

        public override void run_sql(string sql_to_run, ConnectionType connection_type)
        {
            string query = sql_to_run;
            if (string.IsNullOrEmpty(sql_to_run)) return;

            try
            {
                string sql_mode = null;

                // Read The sql_mode to determine ANSI_QUOTED && NO_BACKSLASH_ESCAPES
                using (var dataReader = setup_database_command("SHOW VARIABLES", connection_type, null).ExecuteReader())
                {
                    while (dataReader.Read())
                    {
                        var variable = dataReader.GetString(0);
                        if (variable.Equals("sql_mode", StringComparison.OrdinalIgnoreCase))
                        {
                            sql_mode = dataReader.GetString(1);
                            break;
                        }
                    }
                }

                // create a new parser to parse statements -- http://bugs.mysql.com/bug.php?id=46429
                Parser parser = new Parser(sql_to_run);

                // set ANSI quote mode, may effect delimiter parsing
                parser.AnsiQuotes = sql_mode.IndexOf("ANSI_QUOTES") == 0 ? false : true;

                // parse out and process our SQL statements
                List<ParsedStatement> statements = parser.Parse();
                foreach (var statement in statements)
                {
                    query = statement.Value;

                    if (string.IsNullOrWhiteSpace(statement.Value))
                    {
                        // we don't execute empty statements
                        continue;
                    }

                    if (statement.StatementType.Equals(ParsedStatement.Type.Delimiter)) {
                        // the delimiter is for parsing only
                        continue;
                    }

                    using (var command = setup_database_command(statement.Value, connection_type, null))
                    {
                        Log.bound_to(this).log_a_debug_event_containing(query);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_a_debug_event_containing(
                    "Failure executing query \"{0}\": {1}", 
                    query, 
                    ex.ToString());
                throw ex;
            }
        }

        public override string set_recovery_mode_script(bool simple)
        {
            return string.Empty;
        }

        public override string restore_database_script(string restore_from_path, string custom_restore_options)
        {
            if (restore_from_path == null)
                return string.Empty;
            Log.bound_to(this).log_an_info_event_containing("Restoring from path " + restore_from_path);
            if (!File.Exists(restore_from_path))
                return string.Empty;
            return File.ReadAllText(restore_from_path);
        }
    }
}