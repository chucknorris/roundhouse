using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using Microsoft.Build.Framework;
using log4net.Core;
using log4net.Repository;
using ILogger = Microsoft.Build.Framework.ILogger;

namespace roundhouse.infrastructure.app
{
    using System.IO;
    using builders;
    using containers;
    using containers.custom;
    using cryptography;
    using databases;
    using environments;
    using filesystem;
    using folders;
    using infrastructure.logging;
    using infrastructure.logging.custom;
    using init;
    using logging;
    using migrators;
    using resolvers;
    using System.Linq;

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
            if (string.IsNullOrEmpty(configuration_property_holder.TriggersFolderName))
            {
                configuration_property_holder.TriggersFolderName = ApplicationParameters.default_triggers_folder_name;
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
            if (!configuration_property_holder.EnvironmentNames.Any())
            {
                configuration_property_holder.EnvironmentNames.Add(ApplicationParameters.default_environment_name);
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

            Logger multi_logger = GetMultiLogger(configuration_property_holder);
            
            var file_system = new DotNetFileSystemAccess(configuration_property_holder);
            
            var database = DatabaseBuilder.build(file_system, configuration_property_holder);
            
            // forcing a build of database to initialize connections so we can be sure server/database have values
            database.initialize_connections(configuration_property_holder);
            configuration_property_holder.ServerName = database.server_name;
            configuration_property_holder.DatabaseName = database.database_name;
            configuration_property_holder.ConnectionString = database.connection_string;
            
            var known_folders = KnownFoldersBuilder.build(file_system, configuration_property_holder);
            var log_factory = new MultipleLoggerLogFactory();
            var crypto_service = new MD5CryptographicService();
            var db_migrator = new DefaultDatabaseMigrator(database, crypto_service, configuration_property_holder);
            var version_resolver = VersionResolverBuilder.build(file_system, configuration_property_holder);
            var environment_set = new DefaultEnvironmentSet(configuration_property_holder);
            var initializer = new FileSystemInitializer(known_folders);
            
            HardcodedContainer.Register<ConfigurationPropertyHolder>(configuration_property_holder);
            HardcodedContainer.Register<FileSystemAccess>(file_system);
            HardcodedContainer.Register<Database>(database);
            HardcodedContainer.Register<KnownFolders>(known_folders);
            HardcodedContainer.Register<LogFactory>(log_factory);
            HardcodedContainer.Register<Logger>(multi_logger);
            HardcodedContainer.Register<CryptographicService>(crypto_service);
            HardcodedContainer.Register<DatabaseMigrator>(db_migrator);
            HardcodedContainer.Register<VersionResolver>(version_resolver);
            HardcodedContainer.Register<EnvironmentSet>(environment_set);
            HardcodedContainer.Register<Initializer>(initializer);
           

            return HardcodedContainer.Instance;
        }

        private static Logger GetMultiLogger(ConfigurationPropertyHolder configuration_property_holder)
        {
            IList<Logger> loggers = new List<Logger>();
            
            // This doesn't work on macOS, at least. Try, and fail silently.
            try
            {
                var task = configuration_property_holder as ITask;
                if (task != null)
                {
                    Logger msbuild_logger = new MSBuildLogger(configuration_property_holder);
                    loggers.Add(msbuild_logger);
                }
            }
            catch (FileNotFoundException)
            {}

            Logger log4net_logger = new Log4NetLogger(LogManager.GetLogger(typeof(ApplicationConfiguraton)));
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