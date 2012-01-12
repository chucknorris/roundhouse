namespace roundhouse.infrastructure.app.builders
{
    using System;
    using filesystem;
    using folders;

    public static class KnownFoldersBuilder
    {
        public static KnownFolders build(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
        {
            MigrationsFolder alter_database_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.AlterDatabaseFolderName, false, false, "AlterDatabase");
            MigrationsFolder run_after_create_database_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.RunAfterCreateDatabaseFolderName, true, false, "Run After Create Database");
	        MigrationsFolder run_before_up_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.RunBeforeUpFolderName, false, false, "Run Before Update");
	        MigrationsFolder up_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.UpFolderName, true, false, "Update");
            MigrationsFolder down_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.DownFolderName, true, false, "Down Folder - Nothing to see here. Move along.");
            MigrationsFolder run_first_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.RunFirstAfterUpFolderName, false, false, "Run First After Update");
            MigrationsFolder functions_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.FunctionsFolderName, false, false, "Function");
            MigrationsFolder views_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.ViewsFolderName, false, false, "View");
            MigrationsFolder sprocs_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.SprocsFolderName, false, false, "Stored Procedure");
            MigrationsFolder indexes_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.IndexesFolderName, false, false, "Index");
            MigrationsFolder runAfterOtherAnyTimeScripts_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.RunAfterOtherAnyTimeScriptsFolderName, false, false, "Run after Other Anytime Scripts");
            MigrationsFolder permissions_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory, configuration_property_holder.PermissionsFolderName, false, true, "Permission");

            Folder change_drop_folder = new DefaultFolder(file_system, combine_items_into_one_path(file_system,
                                                                                                   configuration_property_holder.OutputPath,
                                                                                                   "migrations",
                                                                                                   remove_paths_from(configuration_property_holder.DatabaseName,file_system),
                                                                                                   remove_paths_from(configuration_property_holder.ServerName,file_system)),
                                                          get_run_date_time_string());

			return new DefaultKnownFolders(alter_database_folder, run_after_create_database_folder, run_before_up_folder, up_folder, down_folder, run_first_folder, functions_folder, views_folder, sprocs_folder, indexes_folder, runAfterOtherAnyTimeScripts_folder, permissions_folder, change_drop_folder);
        }

        private static string combine_items_into_one_path(FileSystemAccess file_system, params string[] paths)
        {
            return file_system.combine_paths(paths);
        }

        private static string remove_paths_from(string name, FileSystemAccess file_system)
        {
            return file_system.get_file_name_without_extension_from(name);
        }

        private static string get_run_date_time_string()
        {
            return string.Format("{0:yyyyMMdd_HHmmss_ffff}", DateTime.Now);
        }
    }
}