namespace roundhouse.infrastructure
{
    using System;
    using System.Reflection;
    using logging;

    public static class ApplicationParameters
    {
        public static string name = "RoundhousE";
        // defaults
        public static readonly string default_up_folder_name = "up";
        public static readonly string default_down_folder_name = "down";
        public static readonly string default_run_first_after_up_folder_name = "runFirstAfterUp";
        public static readonly string default_functions_folder_name = "functions";
        public static readonly string default_views_folder_name = "views";
        public static readonly string default_sprocs_folder_name = "sprocs";
        public static readonly string default_runAfterOtherAnyTime_folder_name = "runAfterOtherAnyTimeScripts";
        public static readonly string default_permissions_folder_name = "permissions";
        public static readonly string default_environment_name = "LOCAL";
        public static readonly string default_roundhouse_schema_name = "RoundhousE";
        public static readonly string default_version_table_name = "Version";
        public static readonly string default_scripts_run_table_name = "ScriptsRun";
        public static readonly string default_scripts_run_errors_table_name = "ScriptsRunErrors";
        public static readonly string default_version_file = @"_BuildInfo.xml";
        public static readonly string default_version_x_path = @"//buildInfo/version";
        public static readonly string default_files_directory = @".";
        public static readonly string default_server_name = "(local)";
        public static readonly string default_output_path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\" + name;
        public static readonly string default_database_type = "roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver";
        public static readonly string logging_file = @"C:\Temp\RoundhousE\roundhouse.changes.log";
        public static readonly string log4net_configuration_assembly = @"roundhouse";
        public static readonly string log4net_configuration_resource = @"roundhouse.infrastructure.app.logging.log4net.config.xml";
        public static readonly string log4net_configuration_resource_no_console = @"roundhouse.infrastructure.app.logging.log4net.config.no.console.xml";
        public static readonly int default_command_timeout = 60;
        public static readonly int default_restore_timeout = 900;

        public static string get_merged_assembly_name()
        {
            string merged_assembly_name = "rh";
            Log.bound_to(typeof (ApplicationParameters)).log_a_debug_event_containing("The executing assembly is \"{0}\"",
                                                                                      Assembly.GetExecutingAssembly().Location);
            if (Assembly.GetExecutingAssembly().Location.Contains("roundhouse.dll"))
            {
                merged_assembly_name = "roundhouse";
            }

            return merged_assembly_name;
        }

        public static bool is_type_merged_in_this_assembly(string type_to_check)
        {
            bool is_merged = true;
            var assembly = Assembly.GetExecutingAssembly();

            var type = assembly.GetType(type_to_check);
            if (type == null)
            {
                is_merged = false;
            }


            return is_merged;
        }

        public class CurrentMappings
        {
            public static string roundhouse_schema_name = default_roundhouse_schema_name;
            public static string version_table_name = default_version_table_name;
            public static string scripts_run_table_name = default_scripts_run_table_name;
            public static string scripts_run_errors_table_name = default_scripts_run_errors_table_name;
            public static string database_type = default_database_type;
        }
    }
}