using System.Collections.Generic;
using System.Reflection;
using log4net;
using Microsoft.Build.Framework;
using log4net.Core;
using log4net.Repository;
using ILogger = Microsoft.Build.Framework.ILogger;

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
    using init;
    using logging;
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
            if (configuration_property_holder.CommandTimeout == 0)
            {
                configuration_property_holder.CommandTimeout = ApplicationParameters.default_command_timeout;
            }
            if (configuration_property_holder.CommandTimeoutAdmin == 0)
            {
                configuration_property_holder.CommandTimeoutAdmin = ApplicationParameters.default_admin_command_timeout;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.AlterDatabaseFolderName))
            {
                configuration_property_holder.AlterDatabaseFolderName = ApplicationParameters.default_alter_database_folder_name;
            }
            if (string.IsNullOrEmpty(configuration_property_holder.RunAfterCreateDatabaseFolderName))
            {
                configuration_property_holder.RunAfterCreateDatabaseFolderName = ApplicationParameters.default_run_after_create_database_folder_name;
            }
			if (string.IsNullOrEmpty(configuration_property_holder.RunBeforeUpFolderName))
			{
				configuration_property_holder.RunBeforeUpFolderName = ApplicationParameters.default_run_before_up_folder_name;
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
            if (string.IsNullOrEmpty(configuration_property_holder.IndexesFolderName))
            {
                configuration_property_holder.IndexesFolderName = ApplicationParameters.default_indexes_folder_name;
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
            if (!string.IsNullOrEmpty(configuration_property_holder.RestoreFromPath))
            {
                configuration_property_holder.RestoreFromPath = Path.GetFullPath(configuration_property_holder.RestoreFromPath);
            }
            if (configuration_property_holder.RecoveryModeSimple)
            {
                configuration_property_holder.RecoveryMode = RecoveryMode.Simple;
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
            initialize_file_log_appender();
            set_logging_level_debug_when_debug(configuration_property_holder);
        }

        private static InversionContainer build_items_for_container(ConfigurationPropertyHolder configuration_property_holder)
        {
            configuration_property_holder.DatabaseType = DatabaseTypeSynonyms.convert_database_type_synonyms(configuration_property_holder.DatabaseType);

            set_up_current_mappings(configuration_property_holder);

            Logger multiLogger = GetMultiLogger(configuration_property_holder);

            ObjectFactory.Configure(cfg =>
                                        {
                                            cfg.For<ConfigurationPropertyHolder>().Singleton().Use(configuration_property_holder);
                                            cfg.For<FileSystemAccess>().Singleton().Use<WindowsFileSystemAccess>();
                                            cfg.For<Database>().Singleton().Use(context => DatabaseBuilder.build(context.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                            cfg.For<KnownFolders>().Singleton().Use(context => KnownFoldersBuilder.build(context.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                            cfg.For<LogFactory>().Singleton().Use<MultipleLoggerLogFactory>();
                                            //cfg.For<Logger>().Singleton().Use(context => LogBuilder.build(context.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                            cfg.For<Logger>().Use(multiLogger);
                                            cfg.For<CryptographicService>().Singleton().Use<MD5CryptographicService>();
                                            cfg.For<DatabaseMigrator>().Singleton().Use(context => new DefaultDatabaseMigrator(context.GetInstance<Database>(), context.GetInstance<CryptographicService>(), configuration_property_holder));
                                            cfg.For<VersionResolver>().Singleton().Use(
                                                context => VersionResolverBuilder.build(context.GetInstance<FileSystemAccess>(), configuration_property_holder));
                                            cfg.For<Environment>().Singleton().Use(new DefaultEnvironment(configuration_property_holder));
                                            cfg.For<Initializer>().Singleton().Use<FileSystemInitializer>();
                                        });

            // forcing a build of database to initialize connections so we can be sure server/database have values
            Database database = ObjectFactory.GetInstance<Database>();
            database.initialize_connections(configuration_property_holder);
            configuration_property_holder.ServerName = database.server_name;
            configuration_property_holder.DatabaseName = database.database_name;
            configuration_property_holder.ConnectionString = database.connection_string;

            return new StructureMapContainer(ObjectFactory.Container);
        }

        private static Logger GetMultiLogger(ConfigurationPropertyHolder configuration_property_holder)
        {
            IList<Logger> loggers = new List<Logger>();

            var task = configuration_property_holder as ITask;
            if (task != null)
            {
                Logger msbuild_logger = new MSBuildLogger(configuration_property_holder);
                loggers.Add(msbuild_logger);
            }

            Logger log4net_logger = new Log4NetLogger(LogManager.GetLogger("roundhouse"));
            loggers.Add(log4net_logger);

            if (configuration_property_holder.Logger != null && !loggers.Contains(configuration_property_holder.Logger))
            {
                loggers.Add(configuration_property_holder.Logger);
            }

            return new MultipleLogger(loggers);
        }

        private static void initialize_file_log_appender()
        {
            var known_folders = Container.get_an_instance_of<KnownFolders>();

            Log4NetAppender.set_file_appender(known_folders.change_drop.folder_full_path);
        }

        private static void set_logging_level_debug_when_debug(ConfigurationPropertyHolder configuration_property_holder)
        {
            if (configuration_property_holder.Debug)
            {
                ILoggerRepository log_repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
                log_repository.Threshold = Level.Debug;
                foreach (log4net.Core.ILogger log in log_repository.GetCurrentLoggers())
                {
                    var logger = log as log4net.Repository.Hierarchy.Logger;
                    if (logger != null)
                    {
                        logger.Level = Level.Debug;
                    }
                }
            }
        }
    }
}