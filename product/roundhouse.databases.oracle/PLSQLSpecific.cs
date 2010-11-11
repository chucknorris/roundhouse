namespace roundhouse.databases.oracle
{
    public sealed class PLSQLSpecific
    {

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
                                ,repository_path			VarChar(255)	                            NULL
                                ,version			        VarChar(35)	                                NULL
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

        public string insert_script_run_error_parameterized(string roundhouse_schema_name, string scripts_run_errors_table_name)
        {
            return string.Format(
                @"
                    INSERT INTO {0}_{1}
                    (
                        id
                        ,version
                        ,repository_path
                        ,script_name
                        ,text_of_script
                        ,erroneous_part_of_script
                        ,error_message
                        ,entered_by
                    )
                    VALUES
                    (
                        {0}_{1}id.NEXTVAL
                        ,:repository_version
                        ,:repository_path
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
