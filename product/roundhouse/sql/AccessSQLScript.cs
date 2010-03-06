using System;

namespace roundhouse.sql
{
    public class AccessSQLScript : SqlScript
    {
        public string separator_characters_regex
        {
            get { return @"(?<KEEP1>(?:.)*(?:-{2}).*$)|(?<KEEP1>(?:.)*/\*[\S\s]*?\*/)|(?<KEEP1>(?:.)*'(?:\\.|[^'\\])*')|(?<KEEP1>(?:.)*""(?:\\.|[^""\\])*"")|^(?<BATCHSPLITTER>\;)(?<KEEP2>.*)$|(?<KEEP1>^[^\S\n]*.*\s)(?<BATCHSPLITTER>\;)(?<KEEP2>[^\S\n]*.*[^\S\n]*(?:(?:--).*)?$)"; }
        }

        public string create_database(string database_name)
        {
            throw new System.NotImplementedException();
        }

        public string set_recovery_mode(string database_name, bool simple)
        {
            throw new System.NotImplementedException();
        }

        public string restore_database(string database_name, string restore_from_path, string custom_restore_options)
        {
            throw new System.NotImplementedException();
        }

        public string delete_database(string database_name)
        {
            throw new System.NotImplementedException();
        }

        public string create_roundhouse_schema(string roundhouse_schema_name)
        {
            throw new System.NotImplementedException();
        }

        public string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
                @"
                      CREATE TABLE [{0}]
                        (
                            id                  AUTOINCREMENT 
                            ,repository_path    TEXT(255)
                            ,version		    TEXT(255)
                            ,entry_date			DATETIME
                            ,modified_date		DATETIME
                            ,entered_by         TEXT(255)
                        );
                ",
                    version_table_name);
        }

        public string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name, string scripts_run_table_name)
        {
            return string.Format(
                @"
                        CREATE TABLE [{0}]
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
                scripts_run_table_name);
        }

        public string use_database(string database_name)
        {
            throw new System.NotImplementedException();
        }

        public string get_version(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
               @"
                    SELECT TOP 1 version 
                    FROM [{0}]
                    WHERE 
                        repository_path = '{1}' 
                    ORDER BY entry_date DESC;
                ",
               version_table_name, repository_path);
        }

        public string insert_version(string roundhouse_schema_name, string version_table_name, string repository_path, string repository_version, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}]
                    (
                        repository_path
                        ,version
                        ,entered_by
                        ,entry_date	
                        ,modified_date
                    )
                    VALUES
                    (
                        '{1}'
                        ,'{2}'
                        ,'{3}'
                        ,'{4}'
                        ,'{5}'
                    );
                ",
                version_table_name, repository_path, repository_version, user_name,
                DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        }

        public string get_version_id(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
               @"
                   
                    SELECT TOP 1 id 
                    FROM [{0}]
                    WHERE 
                        repository_path = '{1}' 
                    ORDER BY entry_date DESC;
                ",
              version_table_name, repository_path);
        }

        public string get_current_script_hash(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
               @"
                    SELECT TOP 1
                        text_hash
                    FROM [{0}]
                    WHERE script_name = '{1}'
                    ORDER BY entry_date DESC;
                ",
               scripts_run_table_name, script_name
               );
        }

        public string has_script_run(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
                @"
                    SELECT 
                        script_name
                    FROM [{0}]
                    WHERE script_name = '{1}';
                ",
                scripts_run_table_name, script_name
                );
        }

        public string insert_script_run(string roundhouse_schema_name, string scripts_run_table_name, long version_id, string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}] 
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
                        {1}
                        ,'{2}'
                        ,'{3}'
                        ,{4}
                        ,'{5}'
                        ,'{6}'
                        ,'{7}'
                    );
                ",
                scripts_run_table_name, version_id,
                script_name, 
                sql_to_run_hash,
                run_this_script_once ? 1 : 0, user_name,
                DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
        }
    }
}