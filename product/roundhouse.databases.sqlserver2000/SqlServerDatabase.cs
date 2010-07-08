
using System.Text.RegularExpressions;

namespace roundhouse.databases.sqlserver2000
{
    using infrastructure.extensions;
    using sql;

    public class SqlServerDatabase : AdoNetDatabase
    {
        private string connect_options = "Integrated Security";

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