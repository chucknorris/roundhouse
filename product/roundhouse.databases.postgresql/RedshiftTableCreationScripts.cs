namespace roundhouse.databases.postgresql
{
	public sealed class RedshiftTableCreationScripts
	{
        public static string select_exists_roundhouse_schema(string roundhouse_schema_name)
        {
            return $@"    
SELECT
count(*)
FROM pg_catalog.pg_namespace
WHERE lower(nspname) = lower('{roundhouse_schema_name}');";
        }

        public static string create_roundhouse_schema(string roundhouse_schema_name)
        {
            return $@"
CREATE SCHEMA {roundhouse_schema_name}";
        }

        public static string select_exists_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
        {
            return $@"
SELECT
COUNT(*) AS records
FROM pg_tables
WHERE schemaname = lower('{roundhouse_schema_name}') AND tablename = lower('{version_table_name}');";
        }

        public static string create_roundhouse_version_table(string roundhouse_schema_name, string version_table_name)
        {
            var full_version_table_name = $"{roundhouse_schema_name}.{version_table_name}";

            return $@"
CREATE TABLE {full_version_table_name}
(
	id			        INTEGER                 NOT NULL
	,repository_path    VARCHAR(255)	        NULL
	,version		    VARCHAR(50)	            NULL
	,entry_date		    TIMESTAMP       	    NOT NULL DEFAULT current_timestamp
	,modified_date		TIMESTAMP	            NOT NULL DEFAULT current_timestamp
	,entered_by		    VARCHAR(50)	            NULL
);

ALTER TABLE {full_version_table_name} ADD CONSTRAINT {version_table_name}_pk PRIMARY KEY(id);

GRANT SELECT ON TABLE {full_version_table_name} TO public;
";
        }

        public static string select_exists_roundhouse_scripts_run_table(string roundhouse_schema_name, string scripts_run_table_name)
        {
            return $@"SELECT COUNT(*) FROM pg_tables WHERE schemaname = lower('{roundhouse_schema_name}') AND tablename = lower('{scripts_run_table_name}');";
        }

		public static string create_roundhouse_scripts_run_table(string roundhouse_schema_name, string version_table_name, string scripts_run_table_name)
		{
            var full_version_table_name = $"{roundhouse_schema_name}.{version_table_name}";
            var full_scripts_run_table_name = $"{roundhouse_schema_name}.{scripts_run_table_name}";

			return $@"
CREATE TABLE {full_scripts_run_table_name}
(
	 id			        INTEGER                 NOT NULL
	,version_id		    INTEGER			        NULL
	,script_name		VARCHAR(255)		    NULL
	,text_of_script		TEXT			        NULL
	,text_hash		    VARCHAR(512)	        NULL
	,one_time_script	BOOLEAN			        NULL DEFAULT false
	,entry_date		    TIMESTAMP		        NOT NULL DEFAULT current_timestamp
	,modified_date		TIMESTAMP		        NOT NULL DEFAULT current_timestamp
	,entered_by		    VARCHAR(50)		        NULL
);

ALTER TABLE {full_scripts_run_table_name} ADD CONSTRAINT {scripts_run_table_name}_pk PRIMARY KEY(id);

ALTER TABLE {full_scripts_run_table_name} ADD CONSTRAINT {scripts_run_table_name}_{version_table_name}_fk FOREIGN KEY(version_id) REFERENCES {full_version_table_name}(id);

GRANT SELECT ON TABLE {full_scripts_run_table_name} TO public;
";
		}

        public static string select_exists_roundhouse_scripts_run_errors_table(string roundhouse_schema_name, string scripts_run_errors_table_name)
        {
            return $@"SELECT COUNT(*) FROM pg_tables WHERE schemaname = lower('{roundhouse_schema_name}') AND tablename = lower('{scripts_run_errors_table_name}');";
        }

        public static string create_roundhouse_scripts_run_errors_table(string roundhouse_schema_name, string scripts_run_errors_table_name)
		{
            var full_run_errors_table_name = $"{roundhouse_schema_name}.{scripts_run_errors_table_name}";

            return $@"
CREATE TABLE {full_run_errors_table_name}
(
	 id			                INTEGER                 NOT NULL
	,repository_path			VARCHAR(255)	        NULL
	,version				    VARCHAR(50)	            NULL
	,script_name				VARCHAR(255)	        NULL
	,text_of_script				TEXT		            NULL
	,erroneous_part_of_script   TEXT		            NULL
	,error_message				TEXT		            NULL
	,entry_date				    TIMESTAMP	            NOT NULL DEFAULT current_timestamp
	,modified_date				TIMESTAMP	            NOT NULL DEFAULT current_timestamp
	,entered_by				    VARCHAR(50)	            NULL
);

ALTER TABLE {full_run_errors_table_name} ADD CONSTRAINT {scripts_run_errors_table_name}_pk PRIMARY KEY(id);

GRANT SELECT ON TABLE {full_run_errors_table_name} TO public;
";
		}
	}
}