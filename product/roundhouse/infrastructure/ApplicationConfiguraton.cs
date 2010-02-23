using roundhouse.databases;
using roundhouse.infrastructure.extensions;

namespace roundhouse.infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security.Principal;
    using Castle.Windsor;
    using containers;
    using cryptography;
    using environments;
    using filesystem;
    using folders;
    using loaders;
    using logging;
    using logging.custom;
    using migrators;
    using resolvers;
    using Environment = roundhouse.environments.Environment;

    public static class ApplicationConfiguraton
    {
        public static void set_defaults_if_properties_are_not_set(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (string.IsNullOrEmpty(configuration_property_holder.ServerName))
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
            if (configuration_property_holder.RestoreTimeout==0)
            {
                configuration_property_holder.RestoreTimeout = ApplicationParameters.default_restore_timeout;
            }
        }

        public static void build_the_container(ConfigurationPropertyHolder configuration_property_holder)
        {
            Container.initialize_with(null);
            Container.initialize_with(build_items_for_container(configuration_property_holder));
        }

        private static InversionContainer build_items_for_container(ConfigurationPropertyHolder configuration_property_holder)
        {
            configuration_property_holder.DatabaseType = convert_database_type_synonyms(configuration_property_holder.DatabaseType);

            IWindsorContainer windsor_container = new WindsorContainer();

            windsor_container.AddComponent<FileSystemAccess, WindowsFileSystemAccess>();
            windsor_container.Kernel.AddComponentInstance<KnownFolders>(set_up_known_folders(windsor_container.Resolve<FileSystemAccess>(), configuration_property_holder));
            windsor_container.AddComponent<LogFactory, MultipleLoggerLogFactory>();
            windsor_container.Kernel.AddComponentInstance<Logger>(set_up_loggers(windsor_container.Resolve<FileSystemAccess>(), configuration_property_holder, windsor_container.Resolve<KnownFolders>()));

            windsor_container.Kernel.AddComponentInstance<Database>(set_up_database(windsor_container.Resolve<FileSystemAccess>(), configuration_property_holder));
            windsor_container.AddComponent<CryptographicService, MD5CryptographicService>();

            DatabaseMigrator database_migrator = new DefaultDatabaseMigrator(windsor_container.Resolve<Database>(), windsor_container.Resolve<CryptographicService>(),
                                                                             configuration_property_holder.Restore,
                                                                             configuration_property_holder.RestoreFromPath,
                                                                             configuration_property_holder.RestoreCustomOptions,
                                                                             configuration_property_holder.OutputPath,
                                                                             !configuration_property_holder.WarnOnOneTimeScriptChanges);
            windsor_container.Kernel.AddComponentInstance<DatabaseMigrator>(database_migrator);

            windsor_container.Kernel.AddComponentInstance<VersionResolver>(set_up_version_resolvers(windsor_container.Resolve<FileSystemAccess>(), configuration_property_holder));
            windsor_container.Kernel.AddComponentInstance<Environment>(new DefaultEnvironment(configuration_property_holder.EnvironmentName));

            return new containers.custom.WindsorContainer(windsor_container);
        }

        private static KnownFolders set_up_known_folders(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
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
            MigrationsFolder permissions_folder = new DefaultMigrationsFolder(file_system, configuration_property_holder.SqlFilesDirectory,
                                                                              configuration_property_holder.PermissionsFolderName, false, true);
            Folder change_drop_folder = new DefaultFolder(file_system, combine_items_into_one_path(file_system,
                                                                    configuration_property_holder.OutputPath,
                                                                    configuration_property_holder.DatabaseName,
                                                                    configuration_property_holder.ServerName),
                                                         get_run_date_time_string());

            return new DefaultKnownFolders(up_folder, down_folder, run_first_folder, functions_folder, views_folder, sprocs_folder,
                                                                 permissions_folder, change_drop_folder);
        }

        private static string get_run_date_time_string()
        {
            return string.Format("{0:yyyyMMdd_HHmmss_ffff}", DateTime.Now);
        }

        private static Logger set_up_loggers(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder, KnownFolders known_folders)
        {
            IList<Logger> loggers = new List<Logger>();
            Logger nant_logger = new NAntLogger(configuration_property_holder.NAntTask);
            loggers.Add(nant_logger);

            if (configuration_property_holder.MSBuildTask != null)
            {
                Logger msbuild_logger = new MSBuildLogger(configuration_property_holder, configuration_property_holder.MSBuildTask.BuildEngine);
                loggers.Add(msbuild_logger);
            }

            Logger log4net_logger = new Log4NetLogger(configuration_property_holder.Log4NetLogger);
            loggers.Add(log4net_logger);
            Logger file_logger = new FileLogger(
                        combine_items_into_one_path(
                            file_system,
                            known_folders.change_drop.folder_full_path,
                            string.Format("{0}_{1}.log", ApplicationParameters.name, known_folders.change_drop.folder_name)
                            ),
                        file_system
                    );
            //loggers.Add(file_logger);

            return new MultipleLogger(loggers);
        }

        private static string convert_database_type_synonyms(string database_type)
        {
            string database_type_full_name = database_type;

            switch (database_type.to_lower())
            {
                case "2008":
                case "sql2008":
                case "sqlserver2008":
                    database_type_full_name =
                        "roundhouse.databases.sqlserver2008.SqlServerDatabase, roundhouse.databases.sqlserver2008";
                    break;
                case "2005":
                case "sql2005":
                case "sqlserver2005":
                    database_type_full_name =
                       "roundhouse.databases.sqlserver2005.SqlServerDatabase, roundhouse.databases.sqlserver2005";
                    break;
                case "2000":
                case "sql2000":
                case "sqlserver2000":
                    database_type_full_name =
                      "roundhouse.databases.sqlserver2000.SqlServerDatabase, roundhouse.databases.sqlserver2000";
                    throw new NotSupportedException(
                        "Microsoft SQL Server 2000? Really? Like SNL's \"really.\" Really?! Nice try though.");
                    break;
                case "sql" :
                case "sql.net":
                case "sqlserver":
                case "sqlado.net":
                    database_type_full_name =
                       "roundhouse.databases.sqlserver.SqlServerDatabase, roundhouse.databases.sqlserver";
                    break;
                case "mysql":
                    database_type_full_name =
                      "roundhouse.databases.mysql.MySqlDatabase, roundhouse.databases.mysql";
                    break;
                case "oracle":
                    database_type_full_name =
                     "roundhouse.databases.oracle.OracleDatabase, roundhouse.databases.oracle";
                    break;
                case "oledb":
                    database_type_full_name =
                     "roundhouse.databases.oledb.OleDbDatabase, roundhouse.databases.oledb";
                    break;
            }

            return database_type_full_name;
        }

        private static Database set_up_database(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
        {
            Database database_to_migrate;
            try
            {
                database_to_migrate = (Database)DefaultInstanceCreator.create_object_from_string_type(configuration_property_holder.DatabaseType);
            }
            catch (NullReferenceException)
            {
                if (Assembly.GetExecutingAssembly().FullName.Contains("rh"))
                {
                    string database_type = configuration_property_holder.DatabaseType;
                    database_type = database_type.Substring(0, database_type.IndexOf(','));
                    const string console_name = "rh";
                    database_to_migrate = (Database)DefaultInstanceCreator.create_object_from_string_type(database_type + ", " + console_name);
                }
                else
                {
                    throw;
                }
            }

            if (restore_from_file_ends_with_LiteSpeed_extension(file_system, configuration_property_holder.RestoreFromPath))
            {
                database_to_migrate = new SqlServerLiteSpeedDatabase(database_to_migrate);
            }
            database_to_migrate.server_name = configuration_property_holder.ServerName;
            database_to_migrate.database_name = configuration_property_holder.DatabaseName;
            database_to_migrate.connection_string = configuration_property_holder.ConnectionString;
            database_to_migrate.roundhouse_schema_name = configuration_property_holder.SchemaName;
            database_to_migrate.version_table_name = configuration_property_holder.VersionTableName;
            database_to_migrate.scripts_run_table_name = configuration_property_holder.ScriptsRunTableName;
            database_to_migrate.user_name = get_identity_of_person_running_rh();
            database_to_migrate.custom_create_database_script = configuration_property_holder.CreateDatabaseCustomScript;
            database_to_migrate.command_timeout = ApplicationParameters.default_command_timeout;
            database_to_migrate.restore_timeout = configuration_property_holder.RestoreTimeout;

            return database_to_migrate;
        }

        private static string get_identity_of_person_running_rh()
        {
            string identity_of_runner = string.Empty;
            WindowsIdentity windows_identity = WindowsIdentity.GetCurrent();
            if (windows_identity != null)
            {
                identity_of_runner = windows_identity.Name;
            }

            return identity_of_runner;
        }

        private static VersionResolver set_up_version_resolvers(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
        {
            VersionResolver xml_version_finder = new XmlFileVersionResolver(file_system,
                                                                           configuration_property_holder.VersionXPath,
                                                                           configuration_property_holder.VersionFile);
            VersionResolver dll_version_finder = new DllFileVersionResolver(file_system,
                                                                            configuration_property_holder.VersionFile);
            IEnumerable<VersionResolver> resolvers = new List<VersionResolver> { xml_version_finder, dll_version_finder };

            return new ComplexVersionResolver(resolvers);
        }

        private static string combine_items_into_one_path(FileSystemAccess file_system, params string[] paths)
        {
            return file_system.combine_paths(paths);
        }

        private static bool restore_from_file_ends_with_LiteSpeed_extension(FileSystemAccess file_system, string restore_path)
        {
            if (string.IsNullOrEmpty(restore_path)) return false;

            return file_system.get_file_name_without_extension_from(restore_path).ToLower().EndsWith("ls");
        }
    }
}