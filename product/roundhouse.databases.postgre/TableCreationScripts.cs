namespace roundhouse.databases.postgresql
{
	public sealed class TableCreationScripts
	{
		public static string create_roundhouse_schema(string roundhouse_schema_name)
		{
			return string.Format(@"
CREATE FUNCTION CreateRoundHouseSchema(in schemaName varchar) RETURNS void AS $$
DECLARE t_exists integer;
BEGIN
	SELECT INTO t_exists COUNT(*) FROM information_schema.schemata WHERE schema_name = lower($1);
	IF t_exists = 0 THEN
		EXECUTE 'CREATE SCHEMA ' || lower(schemaName);
	END IF;
END;
$$ LANGUAGE 'plpgsql';
SELECT CreateRoundHouseSchema('{0}');
DROP FUNCTION CreateRoundHouseSchema(in schemaName varchar);
", roundhouse_schema_name);
		}

		 public static string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
		 {
		 	return string.Format(@"
CREATE FUNCTION CreateRoundHouseVersionTable(in schName varchar, in tblName varchar) RETURNS void AS $$
DECLARE t_exists integer;
BEGIN
	SELECT INTO t_exists COUNT(*) FROM pg_tables WHERE tablename = lower($2) AND schemaname = lower($1);
	IF t_exists = 0 THEN
		EXECUTE 'CREATE TABLE ' || lower(schName) || '.' || lower(tblName) || '
		( 
			 id					serial			NOT NULL
			,repository_path	varchar(255)	NULL
			,version			varchar(35)		NULL
			,entry_date			timestamp		NOT NULL default current_timestamp
			,modified_date		timestamp		NOT NULL default current_timestamp
			,entered_by			varchar(50)		NULL
		);
		alter table ' || lower(schName) || '.' || lower(tblName) || ' add constraint ' || lower(tblName || '_pk') || ' primary key (id);';
	END IF;	
END;
$$ LANGUAGE 'plpgsql';
SELECT CreateRoundHouseVersionTable('{0}', '{1}');
DROP FUNCTION CreateRoundHouseVersionTable(in schName varchar, in tblName varchar);
", roundhouse_schema_name, version_table_name);
		 }

		public static string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name, string scripts_run_table_name)
		{
			return string.Format(@"
CREATE FUNCTION CreateRoundHouseScriptsRunTable(in schName varchar, in versionTblName varchar, in tblName varchar) RETURNS void AS $$
DECLARE t_exists integer;
BEGIN
	SELECT INTO t_exists COUNT(*) FROM pg_tables WHERE tablename = lower($3) AND schemaname = lower($1);
	IF t_exists = 0 THEN
		EXECUTE 'CREATE TABLE ' || lower(schName) || '.' || lower(tblName) || '
		( 
			 id					serial			NOT NULL
			,version_id			integer			NULL
			,script_name		varchar(255)	NULL
			,text_of_script		text			NULL
			,text_hash			varchar(512)	NULL
			,one_time_script	boolean			NULL default false
			,entry_date			timestamp		NOT NULL default current_timestamp
			,modified_date		timestamp		NOT NULL default current_timestamp
			,entered_by			varchar(50)		NULL
		);
		alter table ' || lower(schName) || '.' || lower(tblName) || ' add constraint ' || lower(tblName || '_pk') || ' primary key (id);
		alter table ' || lower(schName) || '.' || lower(tblName) || ' add constraint ' || lower(tblName || '_' || versionTblName || '_fk') || ' foreign key (version_id) references ' || lower(schName) || '.' || lower(versionTblName) || ' (id);';
	END IF;
END;
$$ LANGUAGE 'plpgsql';
SELECT CreateRoundHouseScriptsRunTable('{0}', '{2}', '{1}');
DROP FUNCTION CreateRoundHouseScriptsRunTable(in schName varchar, in versionTblName varchar, in tblName varchar);
", roundhouse_schema_name, scripts_run_table_name, version_table_name);
		}

		public static string create_roundhouse_scripts_run_errors_table(string roundhouse_schema_name, string scripts_run_errors_table_name)
		{
			return string.Format(@"
CREATE FUNCTION CreateRoundHouseScriptsRunErrorsTable(in schName varchar, in tblName varchar) RETURNS void AS $$
DECLARE t_exists integer;
BEGIN
	SELECT INTO t_exists COUNT(*) FROM pg_tables WHERE tablename = lower($2) AND schemaname = lower($1);
	IF t_exists = 0 THEN
		EXECUTE 'CREATE TABLE ' || lower(schName) || '.' || lower(tblName) || '
		( 
			 id							serial			NOT NULL
			,repository_path			varchar(255)	NULL
			,version					varchar(35)		NULL
			,script_name				varchar(255)	NULL
			,text_of_script				text			NULL
			,erroneous_part_of_script	text			NULL
			,error_message				varchar(255)	NULL
			,entry_date					timestamp		NOT NULL default current_timestamp
			,modified_date				timestamp		NOT NULL default current_timestamp
			,entered_by					varchar(50)		NULL
		);
		alter table ' || lower(schName) || '.' || lower(tblName) || ' add constraint ' || lower(tblName || '_pk') || ' primary key (id);';
	END IF;	
END;
$$ LANGUAGE 'plpgsql';
SELECT CreateRoundHouseScriptsRunErrorsTable('{0}', '{1}');
DROP FUNCTION CreateRoundHouseScriptsRunErrorsTable(in schName varchar, in tblName varchar);
", roundhouse_schema_name, scripts_run_errors_table_name);
		}
	}
}