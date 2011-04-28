namespace roundhouse.infrastructure.app
{
    using System;
    using System.IO;
    using builders;
    using containers;
    using containers.custom;
    using cryptography;
    using databases;
    using environments;
    using extensions;
    using filesystem;
    using folders;
    using infrastructure.logging;
    using infrastructure.logging.custom;
    using migrators;
    using resolvers;
    using StructureMap;
    using Container = roundhouse.infrastructure.containers.Container;
    using Environment = roundhouse.environments.Environment;

    public static class ApplicationConfiguraton
    {
        public static void set_defaults_if_properties_are_not_set(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (string.IsNullOrEmpty(configuration_property_holder.SqlFilesDirectory))
            {
                configuration_property_holder.SqlFilesDirectory = ApplicationParameters.default_files_directory;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.ServerName) && string.IsNullOrEmpty(configuration_property_holder.ConnectionString))
            {
                configuration_property_holder.ServerName = ApplicationParameters.default_server_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.UpFolderName))
            {
                configuration_property_holder.UpFolderName = ApplicationParameters.default_up_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.DownFolderName))
            {
                configuration_property_holder.DownFolderName = ApplicationParameters.default_down_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.RunFirstAfterUpFolderName))
            {
                configuration_property_holder.RunFirstAfterUpFolderName = ApplicationParameters.default_run_first_after_up_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.FunctionsFolderName))
            {
                configuration_property_holder.FunctionsFolderName = ApplicationParameters.default_functions_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.ViewsFolderName))
            {
                configuration_property_holder.ViewsFolderName = ApplicationParameters.default_views_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.SprocsFolderName))
            {
                configuration_property_holder.SprocsFolderName = ApplicationParameters.default_sprocs_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.RunAfterOtherAnyTimeScriptsFolderName))
            {
                configuration_property_holder.RunAfterOtherAnyTimeScriptsFolderName = ApplicationParameters.default_runAfterOtherAnyTime_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.PermissionsFolderName))
            {
                configuration_property_holder.PermissionsFolderName = ApplicationParameters.default_permissions_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.SchemaName))
            {
                configuration_property_holder.SchemaName = ApplicationParameters.default_roundhouse_schema_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.ScriptsRunTableName))
            {
                configuration_property_holder.ScriptsRunTableName = ApplicationParameters.default_scripts_run_table_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.ScriptsRunErrorsTableName))
            {
                configuration_property_holder.ScriptsRunErrorsTableName = ApplicationParameters.default_scripts_run_errors_table_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.VersionTableName))
            {
                configuration_property_holder.VersionTableName = ApplicationParameters.default_version_table_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.VersionFile))
            {
                configuration_property_holder.VersionFile = ApplicationParameters.default_version_file;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.VersionXPath))
            {
                configuration_property_holder.VersionXPath = ApplicationParameters.default_version_x_path;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.EnvironmentName))
            {
                configuration_property_holder.EnvironmentName = ApplicationParameters.default_environment_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.OutputPath))
            {
                configuration_property_holder.OutputPath = ApplicationParameters.default_output_path;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.DatabaseType))
            {
                configuration_property_holder.DatabaseType = ApplicationParameters.default_database_type;
            }
            if (configuration_property_holder.RestoreTimeout == 0)
            {
                configuration_property_holder.RestoreTimeout = ApplicationParameters.default_restore_timeout;
            }

            if (!string.IsNullOrEmpty(configuration_property_holder.RestoreFromPath)) {
                configuration_property_holder.RestoreFromPath = Path.GetFullPath(configuration_property_holder.RestoreFromPath);    
            }
        }

        private static void set_up_current_mappings(ConfigurationPropertyHolder configuration_property_holder)
        {
            ApplicationParameters.CurrentMappings.roundhouse_schema_name = configuration_property_holder.SchemaName;
            ApplicationParameters.CurrentMappings.version_table_name = configuration_property_holder.VersionTableName;
            ApplicationParameters.CurrentMappings.scripts_run_table_name = configuration_property_holder.ScriptsRunTableName;
            ApplicationParameters.CurrentMappings.scripts_run_errors_table_name = configuration_property_holder.ScriptsRunErrorsTableName;
            ApplicationParameters.CurrentMappings.database_type = configuration_property_holder.DatabaseType;
        }

        public static void build_the_container(ConfigurationPropertyHolder configuration_property_holder)
        {
            Container.initialize_with(null);
            Container.initialize_with(build_items_for_container(configuration_property_holder));
        }

        private static InversionContainer build_items_for_container(ConfigurationPropertyHolder configuration_property_holder)
        {
            //this becomes a scan
            configuration_property_holder.DatabaseType = convert_database_type_synonyms(configuration_property_holder.DatabaseType);

            set_up_current_mappings(configuration_property_holder);

            ObjectFactory.Configure(cfg =>
                                        {
                                            cfg.For<ConfigurationPropertyHolder>().Use(configuration_property_holder);
                                            cfg.For<FileSystemAccess>().Use<WindowsFileSystemAccess>();
                                            cfg.For<Database>().Use(
                                                context => DatabaseBuilder.build(context.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                            cfg.For<KnownFolders>().Use(
                                               context => KnownFoldersBuilder.build(context.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                            cfg.For<LogFactory>().Use<MultipleLoggerLogFactory>();
                                            cfg.For<Logger>().Use(
                                                context => LogBuilder.build(context.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                            cfg.For<CryptographicService>().Use<MD5CryptographicService>();
                                            cfg.For<DatabaseMigrator>().Use(context => new DefaultDatabaseMigrator(context.GetInstance<Database>(), context.GetInstance<CryptographicService>(), configuration_property_holder));
                                            cfg.For<VersionResolver>().Use(
                                                context => VersionResolverBuilder.build(context.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                            cfg.For<Environment>().Use(new DefaultEnvironment(configuration_property_holder));
                                        });
            
            // forcing a build of database to initialize connections so we can be sure server/database have values
            Database database = ObjectFactory.GetInstance<Database>();
            database.initialize_connections(configuration_property_holder);
            configuration_property_holder.ServerName = database.server_name;
            configuration_property_holder.DatabaseName = database.database_name;
            configuration_property_holder.ConnectionString = database.connection_string;


            return new StructureMapContainer(ObjectFactory.Container);
        }

        private static string convert_database_type_synonyms(string database_type)
        {
            string database_type_full_name = database_type;

            switch (database_type.to_lower())
            {
                case "2008":
                case "sql2008":
                case "sqlserver2008":
                case "2005":
                case "sql2005":
                case "sqlserver2005":
                case "sql":
                case "sql.net":
                case "sqlserver":
                case "sqlado.net":
                    database_type_full_name =
                        "roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver";
                    break;
                case "2000":
                case "sql2000":
                case "sqlserver2000":
                    database_type_full_name =
                        "roundhouse.databases.sqlserver2000.SqlServerDatabase, roundhouse.databases.sqlserver2000";
                    break;
                case "mysql":
                    database_type_full_name =
                        "roundhouse.databases.mysql.MySqlDatabase, roundhouse.databases.mysql";
                    break;
                case "oracle":
                    database_type_full_name =
                        "roundhouse.databases.oracle.OracleDatabase, roundhouse.databases.oracle";
                    break;
                case "access" :
                    database_type_full_name = "roundhouse.databases.access.AccessDatabase, roundhouse.databases.access";
                    break;
                //case "oledb":
                //    database_type_full_name =
                //        "roundhouse.databases.oledb.OleDbDatabase, roundhouse.databases.oledb";
                //    break;
            }

            return database_type_full_name;
        }
    }
}