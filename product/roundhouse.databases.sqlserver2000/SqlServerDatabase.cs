
using System;
using System.Text.RegularExpressions;
using roundhouse.infrastructure.logging;

namespace roundhouse.databases.sqlserver2000
{
    using infrastructure.extensions;
    using sql;

    public class SqlServerDatabase : AdoNetDatabase
    {
        private string connect_options = "Integrated Security";

        public override void create_roundhouse_schema_if_it_doesnt_exist()
        {
            try
            {
                //schema = user on SQL 2000
                run_sql(sql_scripts.create_roundhouse_schema(roundhouse_schema_name));
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_an_error_event_containing(
                    "User {0} is missing. You need to create this user manually and run RoundhousE with this user.{1}This is only needed on SQL Server 2000 to garantee a smooth upgrade of this DB to later SQL Server versions(2005 or higher). This user will be the name of the schema in later versions, where the user is no longer needed.{1}{2}",
                    roundhouse_schema_name, Environment.NewLine, ex.Message);
                throw;
            }
        }

        public override void initialize_connection()
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

                    if (string.IsNullOrEmpty(database_name) && (part.to_lower().Contains("initial catalog") || part.to_lower().Contains("database")))
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

            master_database_name = "master";
            if (connect_options == "Integrated Security")
            {
                connect_options = "Integrated Security=SSPI;";
            }

            if (string.IsNullOrEmpty(connection_string) || connection_string.to_lower().Contains(database_name.to_lower()))
            {
                connection_string = build_connection_string(server_name, master_database_name, connect_options);
            }

            set_provider_and_sql_scripts();

            admin_connection_string = Regex.Replace(connection_string, "initial catalog=.*?;", "initial catalog=master;");                        
        }

        public override void set_provider_and_sql_scripts()
        {
             provider = "System.Data.SqlClient";
            SqlScripts.sql_scripts_dictionary.TryGetValue("SQLServer2000", out sql_scripts);
            if (sql_scripts == null)
            {
                sql_scripts = SqlScripts.t_sql2000_scripts;
            }
        }

        private static string build_connection_string(string server_name, string database_name, string connection_options)
        {
            return string.Format("Server={0};initial catalog={1};{2}", server_name, database_name, connection_options);
        }
    }
}