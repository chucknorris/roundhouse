using roundhouse.consoles;
namespace roundhouse.databases.mysql
{
    using System;
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.logging;
    using MySql.Data.MySqlClient;
    using System.Collections.Generic;
    using System.IO;
    using System.Globalization;

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
            connectionBuilder.Port = 3306;
            connectionBuilder.UserID = user_name;
            connectionBuilder.Password = password;
            connectionBuilder.AllowUserVariables = true; // Must allow User variables for SPROCs

            return connectionBuilder.ConnectionString;
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
            Log.bound_to(this).log_a_debug_event_containing("MySQL has no database specific tasks. Moving along now...");
        }

        // http://bugs.mysql.com/bug.php?id=46429
        public override void run_sql(string sql_to_run, ConnectionType connection_type)
        {
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

                // Parse the Sql into statements and account for Delimiter changes                
                var list = BreakIntoStatements(sql_to_run, sql_mode.IndexOf("ANSI_QUOTES") != -1, sql_mode.IndexOf("NO_BACKSLASH_ESCAPES") != -1);
                foreach (var statement in list)
                {
                    if (string.IsNullOrEmpty(statement.text))
                        continue;

                    using (var command = setup_database_command(statement.text, connection_type, null))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_a_debug_event_containing("Failure executing command. {0}{1}", Environment.NewLine, ex.ToString());
                throw ex;
            }
        }

        public override string set_recovery_mode_script(bool simple)
        {
            return string.Empty;
        }

        public override string restore_database_script(string restore_from_path, string custom_restore_options)
        {
            throw new NotImplementedException();
        }

        // MySQLScript Helper Functions
        string delimiter = ";";
        private struct ScriptStatement
        {
            public string text;
            public int line;
            public int position;
        }

        private List<int> BreakScriptIntoLines(string query)
        {
            List<int> list = new List<int>();
            StringReader stringReader = new StringReader(query);
            string str = stringReader.ReadLine();
            int num = 0;
            for (; str != null; str = stringReader.ReadLine())
            {
                list.Add(num);
                num += str.Length;
            }
            return list;
        }

        private List<ScriptStatement> BreakIntoStatements(string query, bool ansiQuotes, bool noBackslashEscapes)
        {
            string str1 = this.delimiter;
            int num1 = 0;
            List<ScriptStatement> list = new List<ScriptStatement>();
            List<int> lineNumbers = this.BreakScriptIntoLines(query);
            MySqlTokenizer tokenizer = new MySqlTokenizer(query);
            tokenizer.AnsiQuotes = ansiQuotes;
            tokenizer.BackslashEscapes = !noBackslashEscapes;
            for (string str2 = tokenizer.NextToken(); str2 != null; str2 = tokenizer.NextToken())
            {
                if (!tokenizer.Quoted)
                {
                    if (str2.ToLower(CultureInfo.InvariantCulture) == "delimiter")
                    {
                        tokenizer.NextToken();
                        this.AdjustDelimiterEnd(query, tokenizer);
                        str1 = query.Substring(tokenizer.StartIndex, tokenizer.StopIndex - tokenizer.StartIndex + 1).Trim();
                        num1 = tokenizer.StopIndex;
                    }
                    else
                    {
                        if (str1.StartsWith(str2) && tokenizer.StartIndex + str1.Length <= query.Length && query.Substring(tokenizer.StartIndex, str1.Length) == str1)
                        {
                            str2 = str1;
                            tokenizer.Position = tokenizer.StartIndex + str1.Length;
                            tokenizer.StopIndex = tokenizer.Position;
                        }
                        int num2 = str2.IndexOf(str1, StringComparison.InvariantCultureIgnoreCase);
                        if (num2 != -1)
                        {
                            int num3 = tokenizer.StopIndex - str2.Length + num2;
                            if (tokenizer.StopIndex == query.Length - 1)
                                ++num3;
                            string str3 = query.Substring(num1, num3 - num1);
                            ScriptStatement scriptStatement = new ScriptStatement();
                            scriptStatement.text = str3.Trim();
                            scriptStatement.line = FindLineNumber(num1, lineNumbers);
                            scriptStatement.position = num1 - lineNumbers[scriptStatement.line];
                            list.Add(scriptStatement);
                            num1 = num3 + str1.Length;
                        }
                    }
                }
            }
            if (num1 < query.Length - 1)
            {
                string str2 = query.Substring(num1).Trim();
                if (!string.IsNullOrEmpty(str2))
                {
                    ScriptStatement scriptStatement = new ScriptStatement();
                    scriptStatement.text = str2;
                    scriptStatement.line = FindLineNumber(num1, lineNumbers);
                    scriptStatement.position = num1 - lineNumbers[scriptStatement.line];
                    list.Add(scriptStatement);
                }
            }
            return list;
        }

        private int FindLineNumber(int position, List<int> lineNumbers)
        {
            int index = 0;
            while (index < lineNumbers.Count && position < lineNumbers[index])
                ++index;
            return index;
        }

        private void AdjustDelimiterEnd(string query, MySqlTokenizer tokenizer)
        {
            int stopIndex = tokenizer.StopIndex;
            char c = query[stopIndex];
            while (!char.IsWhiteSpace(c) && stopIndex < query.Length - 1)
                c = query[++stopIndex];
            tokenizer.StopIndex = stopIndex;
            tokenizer.Position = stopIndex;
        }
    }
}