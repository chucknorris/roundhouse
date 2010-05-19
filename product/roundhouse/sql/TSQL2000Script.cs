using System;

namespace roundhouse.sql
{
    public class TSQL2000Script : SqlScript
    {
        public string separator_characters_regex
        {
            get { return @"(?<KEEP1>^(?:.)*(?:-{2}).*$)|(?<KEEP1>/{1}\*{1}[\S\s]*?\*{1}/{1})|(?<KEEP1>'{1}(?:[^']|\n[^'])*?'{1})|(?<KEEP1>\s)(?<BATCHSPLITTER>GO)(?<KEEP2>\s)|(?<KEEP1>\s)(?<BATCHSPLITTER>GO)(?<KEEP2>$)"; }
        }

        public bool can_support_parameters
        {
            get { return true; }
        }

        public bool has_master_database
        {
            get { return true; }
        }

        public string create_database(string database_name)
        {
            return string.Format(
                @"USE Master 
                        IF NOT EXISTS(SELECT * FROM sysdatabases WHERE [name] = '{0}') 
                         BEGIN 
                            CREATE DATABASE [{0}] 
                         END
                        ",
                database_name);
        }

        public string set_recovery_mode(string database_name, bool simple)
        {
            return string.Format(
                @"USE Master 
                   ALTER DATABASE [{0}] SET RECOVERY {1}
                    ",
                database_name, simple ? "SIMPLE" : "FULL");
        }

        public string restore_database(string database_name, string restore_from_path, string custom_restore_options)
        {
            throw new NotImplementedException();
        }

        public string delete_database(string database_name)
        {
            return string.Format(
                @"USE Master 
                        IF EXISTS(SELECT * FROM sysdatabases WHERE [name] = '{0}') 
                        BEGIN 
                            ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE                            
                            DROP DATABASE [{0}] 
                        END",
                database_name);
        }

        public string create_roundhouse_schema(string roundhouse_schema_name)
        {
            return string.Empty;
        }

        public string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
                @"
                    IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='{0}_{1}')
                      BEGIN
                        CREATE TABLE [{0}_{1}]
                        (
                            id                          BigInt			NOT NULL	IDENTITY(1,1)
                            ,repository_path			VarChar(255)	NULL
                            ,version			        VarChar(35)	    NULL
                            ,entry_date					DateTime        NOT NULL	DEFAULT (GetDate())
                            ,modified_date				DateTime        NOT NULL	DEFAULT (GetDate())
                            ,entered_by                 VarChar(50)     NULL
                            ,CONSTRAINT [PK_{0}_{1}_id] PRIMARY KEY CLUSTERED (id) 
                        )
                      END
                ",
                roundhouse_schema_name, version_table_name);
        }

        public string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name, string scripts_run_table_name)
        {
            return string.Format(
                @"
                    IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='{0}_{1}')
                      BEGIN
                        CREATE TABLE [{0}_{1}]
                        (
                            id                          BigInt			NOT NULL	IDENTITY(1,1)
                            ,version_id                 BigInt			NULL
                            ,script_name                VarChar(255)	NULL
                            ,text_of_script             Text        	NULL
                            ,text_hash                  VarChar(512)    NULL
                            ,one_time_script            Bit         	NULL        DEFAULT(0)
                            ,entry_date					DateTime        NOT NULL	DEFAULT (GetDate())
                            ,modified_date				DateTime        NOT NULL	DEFAULT (GetDate())
                            ,entered_by                 VarChar(50)     NULL
                            ,CONSTRAINT [PK_{0}_{1}_id] PRIMARY KEY CLUSTERED (id) 
                        )
                        
                        ALTER TABLE [{0}_{1}] WITH CHECK ADD CONSTRAINT [FK_.{0}_{1}_{2}_version_id] FOREIGN KEY(version_id) REFERENCES [{0}_{1}] (id)

                      END
                ",
                roundhouse_schema_name, scripts_run_table_name, version_table_name);
        }

        public string create_roundhouse_scripts_run_errors_table(string roundhouse_schema_name, string scripts_run_errors_table_name)
        {
            return string.Format(
                @"
                    IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME='{0}_{1}')
                      BEGIN
                        CREATE TABLE [{0}_{1}]
                        (
                            id                          BigInt			NOT NULL	IDENTITY(1,1)
                            ,version_id                 BigInt			NULL
                            ,script_name                VarChar(255)	NULL
                            ,text_of_script             Text        	NULL
                            ,erroneous_part_of_script   Text			NULL
                            ,error_message              VarChar(255)	NULL
                            ,entry_date					DateTime        NOT NULL	DEFAULT (GetDate())
                            ,modified_date				DateTime        NOT NULL	DEFAULT (GetDate())
                            ,entered_by                 VarChar(50)     NULL
                            ,CONSTRAINT [PK_{0}_{1}_id] PRIMARY KEY CLUSTERED (id) 
                        )                                                
                      END
                ",
                roundhouse_schema_name, scripts_run_errors_table_name);
        }

        public string use_database(string database_name)
        {
            return string.Format("USE {0}", database_name);
        }

        public string get_version(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
                @"
                    SELECT TOP 1 version 
                    FROM [{0}_{1}]
                    WHERE 
                        repository_path = '{2}' 
                    ORDER BY entry_date Desc
                ",
                roundhouse_schema_name, version_table_name, repository_path);
        }

        public string get_version_parameterized(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
                @"
                    SELECT TOP 1 version 
                    FROM [{0}_{1}]
                    WHERE 
                        repository_path = @repository_path 
                    ORDER BY entry_date Desc
                ",
                roundhouse_schema_name, version_table_name);
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
                    )
                    VALUES
                    (
                        '{2}'
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
                    INSERT INTO [{0}_{1}] 
                    (
                        repository_path
                        ,version
                        ,entered_by
                    )
                    VALUES
                    (
                        @repository_path
                        ,@repository_version
                        ,@user_name
                    )
                ",
                roundhouse_schema_name, version_table_name);
        }

        public string get_version_id(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
               @"
                   
                    SELECT TOP 1 id 
                    FROM [{0}_{1}]
                    WHERE 
                        repository_path = '{2}' 
                    ORDER BY entry_date Desc
                ",
               roundhouse_schema_name, version_table_name, repository_path);
        }

        public string get_version_id_parameterized(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
               @"
                   
                    SELECT TOP 1 id 
                    FROM [{0}_{1}]
                    WHERE 
                        repository_path = @repository_path
                    ORDER BY entry_date Desc
                ",
               roundhouse_schema_name, version_table_name);
        }

        public string get_current_script_hash(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
                @"
                    SELECT TOP 1
                        text_hash
                    FROM [{0}_{1}]
                    WHERE script_name = '{2}'
                    ORDER BY entry_date Desc
                ",
                roundhouse_schema_name, scripts_run_table_name, script_name
                );
        }

        public string get_current_script_hash_parameterized(string roundhouse_schema_name, string scripts_run_table_name)
        {
            return string.Format(
               @"
                    SELECT TOP 1
                        text_hash
                    FROM [{0}_{1}]
                    WHERE script_name = @script_name
                    ORDER BY entry_date Desc
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
                    FROM [{0}_{1}]
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
                    FROM [{0}_{1}]
                    WHERE script_name = @script_name
                ",
                roundhouse_schema_name, scripts_run_table_name
                );
        }

        public string insert_script_run(string roundhouse_schema_name, string scripts_run_table_name, long version_id, string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}_{1}] 
                    (
                        version_id
                        ,script_name
                        ,text_of_script
                        ,text_hash
                        ,one_time_script
                        ,entered_by
                    )
                    VALUES
                    (
                        {2}
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
                    INSERT INTO [{0}_{1}] 
                    (
                        version_id
                        ,script_name
                        ,text_of_script
                        ,text_hash
                        ,one_time_script
                        ,entered_by
                    )
                    VALUES
                    (
                        @version_id
                        ,@script_name
                        ,@sql_to_run
                        ,@sql_to_run_hash
                        ,@run_this_script_once
                        ,@user_name
                    )
                ",
                roundhouse_schema_name, scripts_run_table_name);
        }

        public string insert_script_run_error(string roundhouse_schema_name, string scripts_run_errors_table_name, long version_id, string script_name, string sql_to_run, string sql_erroneous_part, string error_message, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}_{1}] 
                    (
                        version_id
                        ,script_name
                        ,text_of_script
                        ,erroneous_part_of_script
                        ,error_message
                        ,entered_by
                    )
                    VALUES
                    (
                        {2}
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
                    INSERT INTO [{0}_{1}] 
                    (
                        version_id
                        ,script_name
                        ,text_of_script
                        ,erroneous_part_of_script
                        ,error_message
                        ,entered_by
                    )
                    VALUES
                    (
                        @version_id
                        ,@script_name
                        ,@sql_to_run
                        ,@sql_erroneous_part
                        ,@error_message
                        ,@user_name
                    )
                ",
                roundhouse_schema_name, scripts_run_errors_table_name);
        }
    }
}