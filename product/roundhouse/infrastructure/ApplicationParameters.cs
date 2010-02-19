namespace roundhouse.infrastructure
{
    public static class ApplicationParameters
    {
        public static string name = "RoundhousE";
        // defaults
        public static string default_up_folder_name = "up";
        public static string default_down_folder_name = "down";
        public static string default_run_first_after_up_folder_name = "runFirstAfterUp";
        public static string default_functions_folder_name = "functions";
        public static string default_views_folder_name = "views";
        public static string default_sprocs_folder_name = "sprocs";
        public static string default_permissions_folder_name = "permissions";
        public static string default_environment_name = "LOCAL";
        public static string default_roundhouse_schema_name = "RoundhousE";
        public static string default_version_table_name = "Version";
        public static string default_scripts_run_table_name = "ScriptsRun";
        public static string default_version_file = @"_BuildInfo.xml";
        public static string default_version_x_path = @"//buildInfo/version";
        public static string default_server_name = "(local)";
        public static string default_output_path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData) + @"\" + name;
        public static string default_database_type = "roundhouse.databases.sqlserver2008.SqlServerDatabase, roundhouse.databases.sqlserver2008";
        public static string logging_file = @"C:\Temp\RoundhousE\roundhouse.changes.log";
        public static string log4net_configuration_assembly = @"roundhouse";
        public static string log4net_configuration_resource = @"roundhouse.infrastructure.app.logging.log4net.config.xml";
        public static string log4net_configuration_resource_no_console = @"roundhouse.infrastructure.app.logging.log4net.config.no.console.xml";
        public static int default_restore_timeout = 900;
    }
}