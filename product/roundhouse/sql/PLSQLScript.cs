namespace roundhouse.sql
{
    public sealed class PLSQLScript : SqlScript
    {
        public string separator_characters_regex
        {
            get { return @"(?<KEEP1>^(?:.)*(?:-{2}).*$)|(?<KEEP1>/{1}\*{1}[\S\s]*?\*{1}/{1})|(?<KEEP1>\s)(?<BATCHSPLITTER>;)(?<KEEP2>\s)|(?<KEEP1>\s)(?<BATCHSPLITTER>;)(?<KEEP2>$)"; }
        }

        public bool can_support_parameters
        {
            get { return true; }
        }

        public bool has_master_database
        {
            get { return false; }
        }

        public string create_database(string database_name)
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
                ", database_name);
        }

        public string set_recovery_mode(string database_name, bool simple)
        {
            return string.Empty;
        }

        public string restore_database(string database_name, string restore_from_path, string custom_restore_options)
        {
            return string.Empty;
        }

        public string delete_database(string database_name)
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
            database_name);
        }

        //roundhouse specific 

        public string create_roundhouse_schema(string roundhouse_schema_name)
        {
            return string.Empty;
        }

        public string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
                @"
                    DECLARE
                        tableExists Integer := 0;
                    BEGIN
                        SELECT COUNT(*) INTO tableExists FROM user_objects WHERE object_type = 'TABLE' AND UPPER(object_name) = UPPER('{0}_{1}');
                        IF tableExists = 0 THEN
                            EXECUTE IMMEDIATE 'CREATE TABLE {0}_{1}
                            (
                                id                          Number(19,0)                                NOT NULL
                                ,repository_path			VarChar(255)	                            NULL
                                ,version			        VarChar(35)	                                NULL
                                ,entry_date					Date            DEFAULT CURRENT_TIMESTAMP   NOT NULL	
                                ,modified_date				Date            DEFAULT CURRENT_TIMESTAMP   NOT NULL	
                                ,entered_by                 VarChar(50)                                 NULL
                            )';
                            
                            EXECUTE IMMEDIATE 'ALTER TABLE {0}_{1} ADD (
                                PRIMARY KEY(id)
                            )';

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
                ", roundhouse_schema_name, version_table_name);
        }

        public string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name, string scripts_run_table_name)
        {
            return string.Format(
                @"
                    DECLARE
                        tableExists Integer := 0;
                    BEGIN
                        SELECT COUNT(*) INTO tableExists FROM user_objects WHERE object_type = 'TABLE' AND UPPER(object_name) = UPPER('{0}_{1}');
                        IF tableExists = 0 THEN
                            EXECUTE IMMEDIATE 'CREATE TABLE {0}_{1}
                            (
                                id                          Number(19,0)                                NOT NULL
                                ,version_id                 Number(19,0)	                            NULL
                                ,script_name                VarChar(255)	                            NULL
                                ,text_of_script             Clob       	                                NULL
                                ,text_hash                  VarChar(512)                                NULL
                                ,one_time_script            NUMBER(1,0)     DEFAULT 0                   NULL        
                                ,entry_date					Date            DEFAULT CURRENT_TIMESTAMP   NOT NULL	
                                ,modified_date				Date            DEFAULT CURRENT_TIMESTAMP   NOT NULL
                                ,entered_by                 VarChar(50)                                 NULL                                
                            )';

                            EXECUTE IMMEDIATE 'ALTER TABLE {0}_{1} ADD (
                                PRIMARY KEY(id)
                            )';

                            EXECUTE IMMEDIATE 'CREATE SEQUENCE {0}_{1}id
                            START WITH 1
                            INCREMENT BY 1
                            MINVALUE 1
                            MAXVALUE 999999999999999999999999999
                            CACHE 20
                            NOCYCLE 
                            NOORDER';

                            EXECUTE IMMEDIATE 'ALTER TABLE {0}_{1} ADD (
                                CONSTRAINT FK_{1}_{2}_vid
                                FOREIGN KEY(version_id)
                                REFERENCES {0}_{2}(id)
                            )';
                        END IF;
                    END;
                ",
                roundhouse_schema_name, scripts_run_table_name, version_table_name);
        }

        public string create_roundhouse_scripts_run_errors_table(string roundhouse_schema_name, string scripts_run_errors_table_name)
        {
            return string.Format(
                @"
                    DECLARE
                        tableExists Integer := 0;
                    BEGIN
                        SELECT COUNT(*) INTO tableExists FROM user_objects WHERE object_type = 'TABLE' AND UPPER(object_name) = UPPER('{0}_{1}');
                        IF tableExists = 0 THEN
                            EXECUTE IMMEDIATE 'CREATE TABLE {0}_{1}
                            (
                                id                          Number(19,0)                                NOT NULL
                                ,version_id                 Number(19,0)	                            NULL
                                ,script_name                VarChar(255)	                            NULL
                                ,text_of_script             Clob       	                                NULL
                                ,erroneous_part_of_script   Clob       	                                NULL
                                ,error_message              VarChar(255)	                            NULL                                
                                ,entry_date					Date            DEFAULT CURRENT_TIMESTAMP   NOT NULL	
                                ,modified_date				Date            DEFAULT CURRENT_TIMESTAMP   NOT NULL
                                ,entered_by                 VarChar(50)                                 NULL                                
                            )';

                            EXECUTE IMMEDIATE 'ALTER TABLE {0}_{1} ADD (
                                PRIMARY KEY(id)
                            )';

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
                roundhouse_schema_name, scripts_run_errors_table_name);
        }

        //functions

        public string use_database(string database_name)
        {
            return string.Empty;
        }

        public string get_version(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
                 @"
                    SELECT version
                    FROM (SELECT * FROM {0}_{1}
                            WHERE 
                                repository_path = '{2}'
                            ORDER BY entry_date DESC)
                    WHERE ROWNUM < 2
                ",
                roundhouse_schema_name, version_table_name, repository_path);
        }

        public string get_version_parameterized(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
                 @"
                    SELECT version
                    FROM (SELECT * FROM {0}_{1}
                            WHERE 
                                repository_path = :repository_path
                            ORDER BY entry_date DESC)
                    WHERE ROWNUM < 2
                ",
                roundhouse_schema_name, version_table_name);
        }

        public string insert_version(string roundhouse_schema_name, string version_table_name, string repository_path, string repository_version, string user_name)
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
                        ,'{2}'
                        ,'{3}'
                        ,'{4}'
                    )
                ",
                roundhouse_schema_name, version_table_name, repository_path, repository_version, user_name.Replace(@"'", @"''"));
        }

        public string insert_version_parameterized(string roundhouse_schema_name, string version_table_name)
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

        public string get_version_id(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
               @"
                    SELECT id
                    FROM (SELECT * FROM {0}_{1}
                            WHERE 
                                repository_path = '{2}'
                            ORDER BY entry_date DESC)
                    WHERE ROWNUM < 2
                ",
               roundhouse_schema_name, version_table_name, repository_path);
        }

        public string get_version_id_parameterized(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
               @"
                    SELECT id
                    FROM (SELECT * FROM {0}_{1}
                            WHERE 
                                repository_path = :repository_path
                            ORDER BY entry_date DESC)
                    WHERE ROWNUM < 2
                ",
               roundhouse_schema_name, version_table_name);
        }

        public string get_current_script_hash(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
                @"
                    SELECT text_hash
                    FROM (SELECT * FROM {0}_{1}
                            WHERE 
                                script_name = '{2}'
                            ORDER BY entry_date DESC)
                    WHERE ROWNUM < 2
                ",
                roundhouse_schema_name, scripts_run_table_name, script_name
                );
        }

        public string get_current_script_hash_parameterized(string roundhouse_schema_name, string scripts_run_table_name)
        {
            return string.Format(
                @"
                    SELECT text_hash
                    FROM (SELECT * FROM {0}_{1}
                            WHERE 
                                script_name = :script_name
                            ORDER BY entry_date DESC)
                    WHERE ROWNUM < 2
                ",
                roundhouse_schema_name, scripts_run_table_name
                );
        }

        public string has_script_run(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
                @"
                    SELECT 
                        script_name
                    FROM {0}_{1}
                    WHERE script_name = '{2}'
                ",
                roundhouse_schema_name, scripts_run_table_name, script_name
                );
        }

        public string has_script_run_parameterized(string roundhouse_schema_name, string scripts_run_table_name)
        {
            return string.Format(
                @"
                    SELECT 
                        script_name
                    FROM {0}_{1}
                    WHERE script_name = :script_name
                ",
                roundhouse_schema_name, scripts_run_table_name
                );
        }

        public string insert_script_run(string roundhouse_schema_name, string scripts_run_table_name, long version_id, string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO {0}_{1}
                    (
                        id
                        ,version_id
                        ,script_name
                        ,text_of_script
                        ,text_hash
                        ,one_time_script
                        ,entered_by
                    )
                    VALUES
                    (
                        {0}_{1}id.NEXTVAL
                        ,{2}
                        ,'{3}'
                        ,'{4}'
                        ,'{5}'
                        ,{6}
                        ,'{7}'
                    )
                ",
                roundhouse_schema_name, scripts_run_table_name, version_id,
                script_name, sql_to_run.Replace(@"'", @"''"),
                sql_to_run_hash,
                run_this_script_once ? 1 : 0, user_name.Replace(@"'", @"''"));
        }

        public string insert_script_run_parameterized(string roundhouse_schema_name, string scripts_run_table_name)
        {
            return string.Format(
                @"
                    INSERT INTO {0}_{1}
                    (
                        id
                        ,version_id
                        ,script_name
                        ,text_of_script
                        ,text_hash
                        ,one_time_script
                        ,entered_by
                    )
                    VALUES
                    (
                        {0}_{1}id.NEXTVAL
                        ,:version_id
                        ,:script_name
                        ,:sql_to_run
                        ,:sql_to_run_hash
                        ,:run_this_script_once
                        ,:user_name
                    )
                ",
                roundhouse_schema_name, scripts_run_table_name);
        }

        public string insert_script_run_error(string roundhouse_schema_name, string scripts_run_errors_table_name, long version_id, string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO {0}_{1}
                    (
                        id
                        ,version_id
                        ,script_name
                        ,text_of_script
                        ,erroneous_part_of_script
                        ,error_message
                        ,entered_by
                    )
                    VALUES
                    (
                        {0}_{1}id.NEXTVAL
                        ,{2}
                        ,'{3}'
                        ,'{4}'
                        ,'{5}'
                        ,'{6}'
                        ,'{7}'
                    )
                ",
                roundhouse_schema_name, scripts_run_errors_table_name, version_id,
                script_name, sql_to_run.Replace(@"'", @"''"),
                sql_erroneous_part.Replace(@"'", @"''"),
                error_message, user_name.Replace(@"'", @"''"));
        }

        public string insert_script_run_error_parameterized(string roundhouse_schema_name, string scripts_run_errors_table_name)
        {
            return string.Format(
                @"
                    INSERT INTO {0}_{1}
                    (
                        id
                        ,version_id
                        ,script_name
                        ,text_of_script
                        ,erroneous_part_of_script
                        ,error_message
                        ,entered_by
                    )
                    VALUES
                    (
                        {0}_{1}id.NEXTVAL
                        ,:version_id
                        ,:script_name
                        ,:sql_to_run
                        ,:sql_erroneous_part
                        ,:error_message
                        ,:user_name
                    )
                ",
                roundhouse_schema_name, scripts_run_errors_table_name);
        }
    }
}
