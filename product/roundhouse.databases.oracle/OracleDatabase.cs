using roundhouse.infrastructure.logging;

namespace roundhouse.databases.oracle
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Text.RegularExpressions;
    using infrastructure.app;
    using infrastructure.extensions;
    using parameters;

    public sealed class OracleDatabase : AdoNetDatabase
    {
        private string connect_options = "Integrated Security";

        public override string sql_statement_separator_regex_pattern
        {
            get { return @"(?<KEEP1>^(?:.)*(?:-{2}).*$)|(?<KEEP1>/{1}\*{1}[\S\s]*?\*{1}/{1})|(?<KEEP1>\s)(?<BATCHSPLITTER>;)(?<KEEP2>\s)|(?<KEEP1>\s)(?<BATCHSPLITTER>;)(?<KEEP2>$)"; }
        }

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
                    if (string.IsNullOrEmpty(server_name) && part.to_lower().Contains("data source"))
                    {
                        database_name = part.Substring(part.IndexOf("=") + 1);
                    }

                    //if (string.IsNullOrEmpty(database_name) && (part.to_lower().Contains("user id")))
                    //{
                    //    database_name = part.Substring(part.IndexOf("=") + 1);
                    //}
                }

                if (!connection_string.to_lower().Contains(connect_options.to_lower()))
                {
                    connect_options = string.Empty;
                    foreach (string part in parts)
                    {
                        if (!part.to_lower().Contains("data source"))
                        {
                            connect_options += part + ";";
                        }
                    }
                }
            }
            if (connect_options == "Integrated Security")
            {
                connect_options = "Integrated Security=yes;";
            }

            if (string.IsNullOrEmpty(connection_string))
            {
                connection_string = build_connection_string(database_name, connect_options);
            }

            configuration_property_holder.ConnectionString = connection_string;

            set_provider();
            if (string.IsNullOrEmpty(admin_connection_string))
            {
                admin_connection_string = Regex.Replace(connection_string, "Integrated Security=.*?;", "Integrated Security=yes;");
                admin_connection_string = Regex.Replace(admin_connection_string, "User Id=.*?;", string.Empty);
                admin_connection_string = Regex.Replace(admin_connection_string, "Password=.*?;", string.Empty);
            }
            configuration_property_holder.ConnectionStringAdmin = admin_connection_string;
        }

        private static string build_connection_string(string database_name, string connection_options)
        {
            return string.Format("Data Source={0};{1}", database_name, connection_options);
        }

        public override void set_provider()
        {
            provider = "System.Data.OracleClient";
        }

        public override void run_database_specific_tasks()
        {
            Log.bound_to(this).log_an_info_event_containing("Creating a sequence for the '{0}' table.", version_table_name);
            run_sql(create_sequence_script(version_table_name), ConnectionType.Default);
            Log.bound_to(this).log_an_info_event_containing("Creating a sequence for the '{0}' table.", scripts_run_table_name);
            run_sql(create_sequence_script(scripts_run_table_name), ConnectionType.Default);
            Log.bound_to(this).log_an_info_event_containing("Creating a sequence for the '{0}' table.", scripts_run_errors_table_name);
            run_sql(create_sequence_script(scripts_run_errors_table_name), ConnectionType.Default);
        }

        public string create_sequence_script(string table_name)
        {
            return string.Format(
                @"
                    DECLARE
                        sequenceExists Integer := 0;
                    BEGIN
                        SELECT COUNT(*) INTO sequenceExists FROM user_objects WHERE object_type = 'SEQUENCE' AND UPPER(object_name) = UPPER('{0}_{1}ID');
                        IF sequenceExists = 0 THEN   
                        
                            EXECUTE IMMEDIATE 'CREATE SEQUENCE {0}_{1}id
                            START WITH 1
                            INCREMENT BY 1
                            MINVALUE 1
                            MAXVALUE 999999999999999999999999999
                            CACHE 20
                            NOCYCLE 
                            NOORDER';
                            
                        END IF;
                    END;
              ",
               roundhouse_schema_name, table_name);
        }

        public override long insert_version_and_get_version_id(string repository_path, string repository_version)
        {
            var insert_parameters = new List<IParameter<IDbDataParameter>>
                                        {
                                            create_parameter("repository_path", DbType.AnsiString, repository_path, 255),
                                            create_parameter("repository_version", DbType.AnsiString, repository_version, 35),
                                            create_parameter("user_name", DbType.AnsiString, user_name, 50)
                                        };
            run_sql(insert_version_script(), ConnectionType.Default, insert_parameters);

            var select_parameters = new List<IParameter<IDbDataParameter>> { create_parameter("repository_path", DbType.AnsiString, repository_path, 255) };
            return Convert.ToInt64(run_sql_scalar(get_version_id_script(), ConnectionType.Default, select_parameters));
        }

        public override void run_sql(string sql_to_run, ConnectionType connection_type)
        {
            Log.bound_to(this).log_a_debug_event_containing("Replacing script text \r\n with \n to be compliant with Oracle.");
            // http://www.barrydobson.com/2009/02/17/pls-00103-encountered-the-symbol-when-expecting-one-of-the-following/
            base.run_sql(sql_to_run.Replace("\r\n", "\n"), connection_type);
        }

        protected override object run_sql_scalar(string sql_to_run, ConnectionType connection_type, IList<IParameter<IDbDataParameter>> parameters)
        {
            Log.bound_to(this).log_a_debug_event_containing("Replacing \r\n with \n to be compliant with Oracle.");
            //http://www.barrydobson.com/2009/02/17/pls-00103-encountered-the-symbol-when-expecting-one-of-the-following/
            sql_to_run = sql_to_run.Replace("\r\n", "\n");
            object return_value = new object();

            if (string.IsNullOrEmpty(sql_to_run)) return return_value;

            using (IDbCommand command = setup_database_command(sql_to_run, connection_type, parameters))
            {
                return_value = command.ExecuteScalar();
                command.Dispose();
            }

            return return_value;
        }

        /// <summary>
        /// This DOES NOT use the ADMIN connection. Use sparingly.
        /// </summary>
        private IParameter<IDbDataParameter> create_parameter(string name, DbType type, object value, int? size)
        {
            IDbCommand command = server_connection.underlying_type().CreateCommand();
            var parameter = command.CreateParameter();
            command.Dispose();

            parameter.Direction = ParameterDirection.Input;
            parameter.ParameterName = name;
            parameter.DbType = type;
            parameter.Value = value ?? DBNull.Value;
            if (size != null)
            {
                parameter.Size = size.Value;
            }

            return new AdoNetParameter(parameter);
        }

        public string insert_version_script()
        {
            return string.Format(
                @"
                    INSERT INTO {0}_{1}
                    (
                        id
                        ,repository_path
                        ,version
                        ,entered_by
                    )
                    VALUES
                    (
                        {0}_{1}id.NEXTVAL
                        ,:repository_path
                        ,:repository_version
                        ,:user_name
                    )
                ",
                roundhouse_schema_name, version_table_name);
        }

        public string get_version_id_script()
        {
            return string.Format(
                @"
                    SELECT id
                    FROM (SELECT * FROM {0}_{1}
                            WHERE 
                                NVL(repository_path, '') = NVL(:repository_path, '')
                            ORDER BY entry_date DESC)
                    WHERE ROWNUM < 2
                ",
                roundhouse_schema_name, version_table_name);
        }

        public override string create_database_script()
        {
            return string.Format(
           @"
                DECLARE
                    v_exists Integer := 0;
                BEGIN
                    SELECT COUNT(*) INTO v_exists FROM dba_users WHERE username = '{0}';
                    IF v_exists = 0 THEN
                        EXECUTE IMMEDIATE 'CREATE USER {0} IDENTIFIED BY {0}';
                        EXECUTE IMMEDIATE 'GRANT CREATE SESSION TO {0}';
                        EXECUTE IMMEDIATE 'GRANT RESOURCE TO {0}';                            
                    END IF;
                END;                        
                ", database_name.to_upper());
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
            return string.Format(
            @" 
                DECLARE
                    v_exists Integer := 0;
                BEGIN
                    SELECT COUNT(*) INTO v_exists FROM dba_users WHERE username = '{0}';
                    IF v_exists > 0 THEN
                        EXECUTE IMMEDIATE 'DROP USER {0} CASCADE';
                    END IF;
                END;",
            database_name.to_upper());
        }
    }
}