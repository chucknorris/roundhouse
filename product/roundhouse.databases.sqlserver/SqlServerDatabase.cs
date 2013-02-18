using System.Data.SqlClient;

namespace roundhouse.databases.sqlserver
{
    using System;
    using System.Data;
    using System.Text;
    using System.Text.RegularExpressions;
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.logging;

    public class SqlServerDatabase : AdoNetDatabase
    {
        private string connect_options = "Integrated Security";

        public override string sql_statement_separator_regex_pattern
        {
            get { return @"(?<KEEP1>^\s*--.*$)|(?<KEEP1>/\*[\S\s]*?\*/)|(?<KEEP1>'[^']*')|(?<KEEP1>\s)(?<BATCHSPLITTER>GO)(?<KEEP2>\s|$)"; }
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

            if (connect_options == "Integrated Security")
            {
                connect_options = "Integrated Security=SSPI;";
            }

            if (string.IsNullOrEmpty(connection_string))
            {
                connection_string = build_connection_string(server_name, database_name, connect_options);
            }
            configuration_property_holder.ConnectionString = connection_string;

            set_provider();
            if (string.IsNullOrEmpty(admin_connection_string))
            {
                admin_connection_string = Regex.Replace(connection_string, "initial catalog=.*?;", "initial catalog=master;", RegexOptions.IgnoreCase);
                admin_connection_string = Regex.Replace(admin_connection_string, "database=.*?;", "database=master;", RegexOptions.IgnoreCase);
            }
            configuration_property_holder.ConnectionStringAdmin = admin_connection_string;
        }

        public override void set_provider()
        {
            provider = "System.Data.SqlClient";
        }

        private static string build_connection_string(string server_name, string database_name, string connection_options)
        {
            return string.Format("data source={0};initial catalog={1};{2}", server_name, database_name, connection_options);
        }

        protected override void connection_specific_setup(IDbConnection connection)
        {
            ((SqlConnection)connection).InfoMessage += (sender, e) => Log.bound_to(this).log_a_debug_event_containing("  [SQL PRINT]: {0}{1}", Environment.NewLine, e.Message);
        }

        public override void run_database_specific_tasks()
        {
            Log.bound_to(this).log_an_info_event_containing(" Creating {0} schema if it doesn't exist.", roundhouse_schema_name);
            create_roundhouse_schema_if_it_doesnt_exist();

            Log.bound_to(this).log_a_debug_event_containing("FUTURE ENHANCEMENT: This should remove a user named RoundhousE if one exists (migration from SQL2000 up)");
            //TODO: Delete RoundhousE user if it exists (i.e. migration from SQL2000 to 2005)
        }

        public void create_roundhouse_schema_if_it_doesnt_exist()
        {
            try
            {
                run_sql(create_roundhouse_schema_script(),ConnectionType.Default);
            }
            catch (Exception ex)
            {
                throw;
                //Log.bound_to(this).log_a_warning_event_containing(
                //    "Either the schema has already been created OR {0} with provider {1} does not provide a facility for creating roundhouse schema at this time.{2}{3}",
                //    GetType(), provider, Environment.NewLine, ex.Message);
            }
        }

        public string create_roundhouse_schema_script()
        {
            return string.Format(
                @"
                    IF NOT EXISTS(SELECT * FROM sys.schemas WHERE [name] = '{0}')
                      BEGIN
	                    EXEC('CREATE SCHEMA [{0}]')
                      END
                "
                , roundhouse_schema_name);
        }

        public override string create_database_script()
        {
            return string.Format(
                @"      DECLARE @Created bit
                        SET @Created = 0
                        IF NOT EXISTS(SELECT * FROM sys.databases WHERE [name] = '{0}') 
                         BEGIN 
                            CREATE DATABASE [{0}] 
                            SET @Created = 1
                         END

                        SELECT @Created 
                        ",
                database_name);

            //                            ALTER DATABASE [{0}] MODIFY FILE ( NAME = N'{0}', FILEGROWTH = 10240KB )
        }

        public override string set_recovery_mode_script(bool simple)
        {
            return string.Format(
                @"USE master 
                   ALTER DATABASE [{0}] SET RECOVERY {1}
                    ",
                database_name, simple ? "SIMPLE" : "FULL");
        }

        public override string restore_database_script(string restore_from_path, string custom_restore_options)
        {
            string restore_options = string.Empty;
            if (!string.IsNullOrEmpty(custom_restore_options))
            {
                restore_options = custom_restore_options.to_lower().StartsWith(",") ? custom_restore_options : ", " + custom_restore_options;
            }
            else
            {
                restore_options = get_default_restore_move_options();
            }

            return string.Format(
                @"USE master 
                        ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        
                        RESTORE DATABASE [{0}]
                        FROM DISK = N'{1}'
                        WITH NOUNLOAD
                        , STATS = 10
                        , RECOVERY
                        , REPLACE
                        {2};

                        ALTER DATABASE [{0}] SET MULTI_USER;
                        ",
                database_name, restore_from_path,
                restore_options
                );
        }

        public string get_default_restore_move_options()
        {
            StringBuilder restore_options = new StringBuilder();
            DataTable dt = execute_datatable("select [name],[physical_name] from sys.database_files");
            if (dt != null && dt.Rows.Count != 0)
            {
                foreach (DataRow row in dt.Rows)
                {
                    restore_options.AppendFormat(", MOVE '{0}' TO '{1}'", row["name"], row["physical_name"]);
                }    
            }

            return restore_options.ToString();
        }

        public override string delete_database_script()
        {
            return string.Format(
                @"USE master 
                        IF EXISTS(SELECT * FROM sys.databases WHERE [name] = '{0}' AND source_database_id is NULL) 
                        BEGIN 
                            ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                        END

                        IF EXISTS(SELECT * FROM sys.databases WHERE [name] = '{0}') 
                        BEGIN
                            EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = '{0}' 
                            DROP DATABASE [{0}] 
                        END",
                database_name);
        }

        /// <summary>
        /// Low level hit to query the database for a restore
        /// </summary>
        private DataTable execute_datatable(string sql_to_run)
        {
            DataSet result = new DataSet();

            using (IDbCommand command = setup_database_command(sql_to_run,ConnectionType.Default,null))
            {
                using (IDataReader data_reader = command.ExecuteReader())
                {
                    DataTable data_table = new DataTable();
                    data_table.Load(data_reader);
                    data_reader.Close();
                    data_reader.Dispose();

                    result.Tables.Add(data_table);
                }
                command.Dispose();
            }

            return result.Tables.Count == 0 ? null : result.Tables[0];
        }

    }
}