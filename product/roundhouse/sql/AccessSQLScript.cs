using System;

namespace roundhouse.sql
{
    public class AccessSQLScript : SqlScript
    {
        public string separator_characters_regex
        {
            get { return @"(?<KEEP1>^(?:.)*(?:-{2}).*$)|(?<KEEP1>/{1}\*{1}[\S\s]*?\*{1}/{1})|(?<KEEP1>'{1}(?:[^']|\n[^'])*?'{1})|(?<KEEP1>\s)(?<BATCHSPLITTER>\;)(?<KEEP2>\s)|(?<KEEP1>\s)(?<BATCHSPLITTER>\;)(?<KEEP2>$)"; }
        }

        public bool can_support_parameters
        {
            get { return false; }
        }

        public bool can_support_ddl_transactions
        {
            get { return false; }
        }

        public bool has_master_database
        {
            get { return false; }
        }

        public string create_database(string database_name)
        {
            throw new NotSupportedException("Access does not have a facility for creating a database.");
        }

        public string set_recovery_mode(string database_name, bool simple)
        {
            throw new NotSupportedException("Access does not have a recovery mode.");
        }

        public string restore_database(string database_name, string restore_from_path, string custom_restore_options)
        {
            throw new NotSupportedException("Access does not have a facility for restoring a database.");
        }

        public string delete_database(string database_name)
        {
            throw new NotSupportedException("Access does not have a facility for removing a database.");
        }

        public string create_roundhouse_schema(string roundhouse_schema_name)
        {
            throw new NotSupportedException("Access does not have a facility for creating schemas.");
        }

        public string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
                @"
                      CREATE TABLE [{0}_{1}]
                        (
                            id                  AUTOINCREMENT 
                            ,repository_path    TEXT(255)
                            ,version		    TEXT(255)
                            ,entry_date			DATETIME
                            ,modified_date		DATETIME
                            ,entered_by         TEXT(255)
                        );
                ",
                    roundhouse_schema_name, version_table_name);
        }

        public string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name, string scripts_run_table_name)
        {
            return string.Format(
                @"
                        CREATE TABLE [{0}_{1}]
                        (
                            id                          AUTOINCREMENT
                            ,version_id                 LONG
                            ,script_name                TEXT(255)
                            ,text_hash                  TEXT(255)
                            ,one_time_script            SHORT
                            ,entry_date					DATETIME
                            ,modified_date				DATETIME
                            ,entered_by                 TEXT(255)
                        );
                ",
                roundhouse_schema_name, scripts_run_table_name);
        }

        public string create_roundhouse_scripts_run_errors_table(string roundhouse_schema_name, string scripts_run_errors_table_name)
        {
            return string.Format(
                @"
                        CREATE TABLE [{0}_{1}]
                        (
                            id                          AUTOINCREMENT
                            ,repository_path            TEXT(255)
                            ,version		            TEXT(255)
                            ,script_name                TEXT(255)
							,error_message				TEXT(255)
                            ,entry_date					DATETIME
                            ,modified_date				DATETIME
                            ,entered_by                 TEXT(255)
                        );
                ",
                roundhouse_schema_name, scripts_run_errors_table_name);
        }

        public string use_database(string database_name)
        {
            throw new NotSupportedException("Access does not have a facility for using a database.");
        }

        public string get_version(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
               @"
                    SELECT TOP 1 version 
                    FROM [{0}_{1}]
                    WHERE 
                        repository_path = '{2}' 
                    ORDER BY entry_date DESC;
                ",
               roundhouse_schema_name, version_table_name, repository_path.Substring(0, 255));
        }

        public string get_version_parameterized(string roundhouse_schema_name, string version_table_name)
        {
            throw new NotSupportedException("Access has not been tested to support parameters.");
        }

        public string insert_version(string roundhouse_schema_name, string version_table_name, string repository_path, string repository_version, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}_{1}]
                    (
                        repository_path
                        ,version
                        ,entered_by
                        ,entry_date	
                        ,modified_date
                    )
                    VALUES
                    (
                        '{2}'
                        ,'{3}'
                        ,'{4}'
                        ,'{5}'
                        ,'{6}'
                    );
                ",
                roundhouse_schema_name, version_table_name, repository_path.Substring(0, 255), repository_version.Substring(0, 255), user_name.Replace(@"'", @"''"),
                DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        }

        public string insert_version_parameterized(string roundhouse_schema_name, string version_table_name)
        {
            throw new NotSupportedException("Access has not been tested to support parameters.");
        }

        public string get_version_id(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
               @"
                   
                    SELECT TOP 1 id 
                    FROM [{0}_{1}]
                    WHERE 
                        repository_path = '{2}' 
                    ORDER BY entry_date DESC;
                ",
              roundhouse_schema_name, version_table_name, repository_path.Substring(0, 255));
        }

        public string get_version_id_parameterized(string roundhouse_schema_name, string version_table_name)
        {
            throw new NotSupportedException("Access has not been tested to support parameters.");
        }

        public string get_current_script_hash(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
               @"
                    SELECT TOP 1
                        text_hash
                    FROM [{0}_{1}]
                    WHERE script_name = '{2}'
                    ORDER BY entry_date DESC;
                ",
               roundhouse_schema_name, scripts_run_table_name, script_name.Substring(0, 255)
               );
        }

        public string get_current_script_hash_parameterized(string roundhouse_schema_name, string scripts_run_table_name)
        {
            throw new NotSupportedException("Access has not been tested to support parameters.");
        }

        public string has_script_run(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
                @"
                    SELECT 
                        script_name
                    FROM [{0}_{1}]
                    WHERE script_name = '{2}';
                ",
                roundhouse_schema_name, scripts_run_table_name, script_name.Substring(0, 255)
                );
        }

        public string has_script_run_parameterized(string roundhouse_schema_name, string scripts_run_table_name)
        {
            throw new NotSupportedException("Access has not been tested to support parameters.");
        }

        public string insert_script_run(string roundhouse_schema_name, string scripts_run_table_name, long version_id, string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}_{1}] 
                    (
                        version_id
                        ,script_name
                        ,text_hash
                        ,one_time_script
                        ,entered_by
                        ,entry_date	
                        ,modified_date
                    )
                    VALUES
                    (
                        {2}
                        ,'{3}'
                        ,'{4}'
                        ,{5}
                        ,'{6}'
                        ,'{7}'
                        ,'{8}'
                    );
                ",
                roundhouse_schema_name, scripts_run_table_name, version_id,
                script_name.Substring(0, 255),
                sql_to_run_hash,
                run_this_script_once ? 1 : 0, user_name.Replace(@"'", @"''"),
                DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        }

        public string insert_script_run_parameterized(string roundhouse_schema_name, string scripts_run_table_name)
        {
            throw new NotSupportedException("Access has not been tested to support parameters.");
        }

        public string insert_script_run_error(string roundhouse_schema_name, string scripts_run_errors_table_name, string repository_version, string repository_path, string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}_{1}] 
                    (
                        version
                        ,repository_path
                        ,script_name
                        ,error_message
                        ,entered_by
                        ,entry_date	
                        ,modified_date
                    )
                    VALUES
                    (
                        '{2}'
                        ,'{3}'
                        ,'{4}'
                        ,'{5}'
                        ,'{6}'
                        ,'{7}'
                        ,'{8}'
                    );
                ",
                roundhouse_schema_name, scripts_run_errors_table_name, 
                repository_version, repository_path,
                script_name.Substring(0, 255),
                error_message.Substring(0,255),
                user_name.Replace(@"'", @"''"),
                DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        }

        public string insert_script_run_error_parameterized(string roundhouse_schema_name, string scripts_run_errors_table_name)
        {
            throw new NotSupportedException("Access has not been tested to support parameters.");
        }
    }
}