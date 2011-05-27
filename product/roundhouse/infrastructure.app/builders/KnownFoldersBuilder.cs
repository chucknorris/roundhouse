namespace roundhouse.infrastructure.app.builders
{
    using System;
    using filesystem;
    using folders;

    public static class KnownFoldersBuilder
    {
        public static KnownFolders build(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
        {
            MigrationsFolder up_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                     configuration_property_holder.UpFolderName, true, false);
            MigrationsFolder down_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                       configuration_property_holder.DownFolderName, true, false);
            MigrationsFolder run_first_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                            configuration_property_holder.RunFirstAfterUpFolderName, false, false);
            MigrationsFolder functions_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                            configuration_property_holder.FunctionsFolderName, false, false);
            MigrationsFolder views_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                        configuration_property_holder.ViewsFolderName, false, false);
            MigrationsFolder sprocs_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                         configuration_property_holder.SprocsFolderName, false, false);

            MigrationsFolder runAfterOtherAnyTimeScripts_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                         configuration_property_holder.RunAfterOtherAnyTimeScriptsFolderName, false, false);

            MigrationsFolder permissions_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                              configuration_property_holder.PermissionsFolderName, false, true);
            Folder change_drop_folder = new DefaultFolder(file_system, combine_items_into_one_path(file_system,
                                                                                                   configuration_property_holder.OutputPath,
                                                                                                   configuration_property_holder.DatabaseName,
                                                                                                   configuration_property_holder.ServerName),
                                                          get_run_date_time_string());

            return new DefaultKnownFolders(up_folder, down_folder, run_first_folder, functions_folder, views_folder, sprocs_folder, 
                runAfterOtherAnyTimeScripts_folder, permissions_folder, change_drop_folder);
        }

        private static string combine_items_into_one_path(FileSystemAccess file_system, params string[] paths)
        {
            return file_system.combine_paths(paths);
        }


        private static string get_run_date_time_string()
        {
            return string.Format("{0:yyyyMMdd_HHmmss_ffff}", DateTime.Now);
        }
    }
}