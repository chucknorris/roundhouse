namespace roundhouse.sql
{
    using infrastructure.extensions;

    public sealed class TSQLScript : SqlScript
    {
        public string separator_characters_regex
        {
            get { return @"(?<!--[\s\w\=\'\.\>\?\<\(\)\[\]\-\*\;]*[^\r\n\f]|\w|--)[\s]*GO[\s]*$(?![\s\w\=\'\.\>\?\<\(\)\[\]\-\*\;\r\n\f]*\*\/)"; }
        }
        
        public string create_database(string database_name)
        {
            return string.Format(
                @"USE Master 
                        IF NOT EXISTS(SELECT * FROM sys.databases WHERE [name] = '{0}') 
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
            string restore_options = string.Empty;
            if (!string.IsNullOrEmpty(custom_restore_options))
            {
                restore_options = custom_restore_options.to_lower().StartsWith(",") ? custom_restore_options : ", " + custom_restore_options;
            }

            return string.Format(
                @"USE Master 
                        ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                        
                        RESTORE DATABASE [{0}]
                        FROM DISK = N'{1}'
                        WITH NOUNLOAD
                        , STATS = 10
                        , RECOVERY
                        , REPLACE
                        {2};

                        ALTER DATABASE [{0}] SET MULTI_USER;
                        --DBCC SHRINKDATABASE ([{0}]);
                        ",
                database_name, restore_from_path,
                restore_options
                );
        }

        public string delete_database(string database_name)
        {
            return string.Format(
                @"USE Master 
                        IF EXISTS(SELECT * FROM sys.databases WHERE [name] = '{0}') 
                        BEGIN 
                            ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE
                            EXEC msdb.dbo.sp_delete_database_backuphistory @database_name = '{0}' 
                            DROP DATABASE [{0}] 
                        END",
                database_name);
        }

        //roundhouse specific 

        public string create_roundhouse_schema(string roundhouse_schema_name)
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

        public string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
        {
            return string.Format(
                @"
                    IF NOT EXISTS(SELECT * FROM sys.tables WHERE [name] = '{1}')
                      BEGIN
                        CREATE TABLE [{0}].[{1}]
                        (
                            id                          BigInt			NOT NULL	IDENTITY(1,1)
                            ,repository_path			VarChar(255)	NULL
                            ,version			        VarChar(35)	    NULL
                            ,entry_date					DateTime        NOT NULL	DEFAULT (GetDate())
                            ,modified_date				DateTime        NOT NULL	DEFAULT (GetDate())
                            ,entered_by                 VarChar(50)     NULL
                            ,CONSTRAINT [PK_{1}_id] PRIMARY KEY CLUSTERED (id) 
                        )
                      END
                ",
                roundhouse_schema_name, version_table_name);
        }

        public string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name, string scripts_run_table_name)
        {
            return string.Format(
                @"
                    IF NOT EXISTS(SELECT * FROM sys.tables WHERE [name] = '{1}')
                      BEGIN
                        CREATE TABLE [{0}].[{1}]
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
                            ,CONSTRAINT [PK_{1}_id] PRIMARY KEY CLUSTERED (id) 
                        )
                        
                        ALTER TABLE [{0}].[{1}] WITH CHECK ADD CONSTRAINT [FK_.{1}_{2}_version_id] FOREIGN KEY(version_id) REFERENCES [{0}].[{2}] (id)

                      END
                ",
                roundhouse_schema_name, scripts_run_table_name, version_table_name);

        }

        //functions

        public string use_database(string database_name)
        {
            return string.Format("USE {0}", database_name);
        }

        public string get_version(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
                @"
                    SELECT TOP 1 version 
                    FROM [{0}].[{1}]
                    WHERE 
                        repository_path = '{2}' 
                    ORDER BY entry_date Desc
                ",
                roundhouse_schema_name, version_table_name, repository_path);
        }

        public string insert_version(string roundhouse_schema_name, string version_table_name, string repository_path, string repository_version, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}].[{1}] 
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
                roundhouse_schema_name, version_table_name, repository_path, repository_version, user_name);

        }

        public string get_version_id(string roundhouse_schema_name, string version_table_name, string repository_path)
        {
            return string.Format(
               @"
                   
                    SELECT TOP 1 id 
                    FROM [{0}].[{1}]
                    WHERE 
                        repository_path = '{2}' 
                    ORDER BY entry_date Desc
                ",
               roundhouse_schema_name, version_table_name, repository_path);
        }

        public string get_current_script_hash(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
                @"
                    SELECT TOP 1
                        text_hash
                    FROM [{0}].[{1}]
                    WHERE script_name = '{2}'
                    ORDER BY entry_date Desc
                ",
                roundhouse_schema_name, scripts_run_table_name, script_name
                );
        }

        public string has_script_run(string roundhouse_schema_name, string scripts_run_table_name, string script_name)
        {
            return string.Format(
                @"
                    SELECT 
                        script_name
                    FROM [{0}].[{1}]
                    WHERE script_name = '{2}'
                ",
                roundhouse_schema_name, scripts_run_table_name, script_name
                );
        }

        public string insert_script_run(string roundhouse_schema_name, string scripts_run_table_name, long version_id, string script_name, string sql_to_run, string sql_to_run_hash, bool run_this_script_once, string user_name)
        {
            return string.Format(
                @"
                    INSERT INTO [{0}].[{1}] 
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
                run_this_script_once ? 1 : 0, user_name);
        }
    }
}