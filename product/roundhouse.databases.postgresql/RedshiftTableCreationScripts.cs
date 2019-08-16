namespace roundhouse.databases.postgresql
{
	public sealed class RedshiftTableCreationScripts
	{
        public static string create_roundhouse_schema(string roundhouse_schema_name)
        {
            return $@"
CREATE OR REPLACE PROCEDURE sp_CreateRoundHouseSchema (schemaName IN VARCHAR(100))
AS $$
DECLARE
    t_exist INTEGER;
BEGIN
​
    SELECT
    INTO t_exist
          count(*)
    FROM pg_catalog.pg_namespace
    WHERE lower(nspname) = lower(schemaName);
​
    IF t_exist = 0 THEN
        EXECUTE 'CREATE SCHEMA '|| lower(schemaName);
    END IF;
END;
$$ LANGUAGE plpgsql;
​
CALL sp_CreateRoundHouseSchema ('{roundhouse_schema_name}');
​
DROP PROCEDURE sp_CreateRoundHouseSchema (schemaName IN VARCHAR(100));
";
        }

        public static string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
		 {
		 	return $@"
CREATE OR REPLACE PROCEDURE sp_CreateRoundHouseVersionTable (schName IN VARCHAR(100), tblName VARCHAR(100))
AS $$
DECLARE
    t_exists integer;
    t_user varchar(255);
    t_table varchar(255);
BEGIN
   	SELECT INTO t_exists
    COUNT(*) AS records
    FROM pg_tables
    WHERE schemaname = lower(schName) AND tablename = lower(tblName);
​
    SELECT current_user INTO t_user;
    SELECT lower(schName) || '.' || lower(tblName) INTO t_table;
​
    IF t_exists = 0 THEN
        EXECUTE 'CREATE TABLE ' || t_table || '
		(
		    id			        INTEGER IDENTITY(1,1)	NOT NULL
			,repository_path    VARCHAR(255)	        NULL
			,version		    VARCHAR(50)	            NULL
			,entry_date		    TIMESTAMP       	    NOT NULL DEFAULT current_timestamp
			,modified_date		TIMESTAMP	            NOT NULL DEFAULT current_timestamp
			,entered_by		    VARCHAR(50)	            NULL
		)';
        EXECUTE 'alter table ' || t_table || ' add constraint ' || replace(t_table, '.', '_') || '_pk' || ' primary key (id);';
		EXECUTE 'GRANT SELECT ON TABLE ' || t_table || ' TO public;';
​
    END IF;
END;
$$ LANGUAGE plpgsql;
​
CALL sp_CreateRoundHouseVersionTable ('{roundhouse_schema_name}','{version_table_name}');
​
DROP PROCEDURE sp_CreateRoundHouseVersionTable (schName IN VARCHAR(100), tblName VARCHAR(100) );
";
		 }

		public static string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name, string scripts_run_table_name)
		{
			return $@"
CREATE OR REPLACE PROCEDURE sp_CreateRoundHouseScriptsRunTable (schName IN VARCHAR(100), versionTblName IN VARCHAR(100), tblName IN VARCHAR(100))
AS $$
DECLARE
    t_exists INTEGER;
    t_user VARCHAR(255);
    t_table VARCHAR(255);
    t_version_table VARCHAR(255);
BEGIN
	SELECT INTO t_exists COUNT(*) FROM pg_tables WHERE schemaname = lower(schName) AND tablename = lower(tblName);
	SELECT current_user INTO t_user;
	SELECT lower(schName) || '.' || lower(tblName) INTO t_table;
	SELECT lower(schName) || '.' || lower(versionTblName) INTO t_version_table;
​
	IF t_exists = 0 THEN
		EXECUTE 'CREATE TABLE ' || t_table || '
		(
			 id			        INTEGER IDENTITY(1,1)   NOT NULL
			,version_id		    INTEGER			        NULL
		    ,script_name		VARCHAR(255)		    NULL
			,text_of_script		TEXT			        NULL
			,text_hash		    VARCHAR(512)	        NULL
			,one_time_script	BOOLEAN			        NULL DEFAULT false
			,entry_date		    TIMESTAMP		        NOT NULL DEFAULT current_timestamp
			,modified_date		TIMESTAMP		        NOT NULL DEFAULT current_timestamp
			,entered_by		    VARCHAR(50)		        NULL
		);';
​
		EXECUTE 'alter table ' || t_table || ' add constraint ' || replace(t_table, '.', '_') || '_pk' || ' primary key (id);';
		EXECUTE 'alter table ' || t_table || ' add constraint ' || replace(t_table, '.', '_') || '_' || versionTblName || '_fk' || ' foreign key (version_id) references ' || t_version_table || ' (id);';
		EXECUTE 'GRANT SELECT ON TABLE ' || t_table || ' TO public;';
	END IF;
END;
$$ LANGUAGE 'plpgsql';
​
CALL sp_CreateRoundHouseScriptsRunTable ('{roundhouse_schema_name}','{version_table_name}','{scripts_run_table_name}');
​
DROP PROCEDURE sp_CreateRoundHouseScriptsRunTable (schName IN VARCHAR(100), versionTblName IN VARCHAR(100), tblName IN VARCHAR(100));
";
		}

		public static string create_roundhouse_scripts_run_errors_table(string roundhouse_schema_name, string scripts_run_errors_table_name)
		{
			return $@"
CREATE OR REPLACE PROCEDURE sp_CreateRoundHouseScriptsRunErrorsTable (schName IN VARCHAR(100), tblName IN VARCHAR(100)) AS
$$
DECLARE
    t_exists INTEGER;
    t_user VARCHAR(255);
    t_table VARCHAR(255);
BEGIN
	SELECT INTO t_exists COUNT(*) FROM pg_tables WHERE schemaname = lower(schName) AND tablename = lower(tblName);
	SELECT current_user INTO t_user;
	SELECT lower(schName) || '.' || lower(tblName) INTO t_table;

	IF t_exists = 0 THEN
		EXECUTE 'CREATE TABLE ' || t_table || '
		(
			 id			                INTEGER IDENTITY(1,1)   NOT NULL
			,repository_path			VARCHAR(255)	        NULL
			,version				    VARCHAR(50)	            NULL
			,script_name				VARCHAR(255)	        NULL
			,text_of_script				TEXT		            NULL
		    ,erroneous_part_of_script   TEXT		            NULL
			,error_message				TEXT		            NULL
			,entry_date				    TIMESTAMP	            NOT NULL DEFAULT current_timestamp
			,modified_date				TIMESTAMP	            NOT NULL DEFAULT current_timestamp
			,entered_by				    VARCHAR(50)	            NULL
		);';

		EXECUTE 'alter table ' || t_table || ' add constraint ' || replace(t_table, '.', '_') || '_pk' || ' primary key (id);';
		EXECUTE 'GRANT SELECT ON TABLE ' || t_table || ' TO public;';
	END IF;
END;
$$ LANGUAGE 'plpgsql';

CALL sp_CreateRoundHouseScriptsRunErrorsTable ('{roundhouse_schema_name}','{scripts_run_errors_table_name}');

DROP PROCEDURE sp_CreateRoundHouseScriptsRunErrorsTable(schName IN varchar (100), tblName IN varchar(100));
";
		}
	}
}