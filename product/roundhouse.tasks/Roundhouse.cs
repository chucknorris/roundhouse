using roundhouse.infrastructure.app.logging;

namespace roundhouse.tasks
{
    using System;
    using folders;
    using infrastructure.app;
    using infrastructure.containers;
    using infrastructure.filesystem;
    using log4net;
    using Microsoft.Build.Framework;
    using migrators;
    using resolvers;
    using runners;
    using Environment = environments.Environment;

    public sealed class Roundhouse : ITask, ConfigurationPropertyHolder
    {
        private readonly ILog the_logger = LogManager.GetLogger(typeof(Roundhouse));

        #region MSBuild

        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        /// <summary>
        /// The function for the MSBuild task that actually does the task
        /// </summary>
        /// <returns>true if the task is successful</returns>
        bool ITask.Execute()
        {
            run_the_task();
            return true;
        }

        #endregion

        #region Properties

        public ILog Log4NetLogger
        {
            get { return the_logger; }
        }

        public ITask MSBuildTask
        {
            get { return this; }
        }

        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        public string ConnectionString { get; set; }

        public string ConnectionStringAdmin { get; set; }

        public string SqlFilesDirectory { get; set; }

        public string RepositoryPath { get; set; }

        public string VersionFile { get; set; }

        public string VersionXPath { get; set; }

        public string UpFolderName { get; set; }

        public string DownFolderName { get; set; }

        public string RunFirstAfterUpFolderName { get; set; }

        public string FunctionsFolderName { get; set; }

        public string ViewsFolderName { get; set; }

        public string SprocsFolderName { get; set; }

        public string PermissionsFolderName { get; set; }

        public string SchemaName { get; set; }

        public string VersionTableName { get; set; }

        public string ScriptsRunTableName { get; set; }

        public string ScriptsRunErrorsTableName { get; set; }

        public string EnvironmentName { get; set; }

        public bool Restore { get; set; }

        public string RestoreFromPath { get; set; }

        public string RestoreCustomOptions { get; set; }

        public int RestoreTimeout { get; set; }

        public string CreateDatabaseCustomScript { get; set; }

        public bool Drop { get; set; }

        public bool DoNotCreateDatabase { get; set; }

        public string OutputPath { get; set; }

        public bool WarnOnOneTimeScriptChanges { get; set; }

        public bool Silent { get; set; }

        public string DatabaseType { get; set; }

        public bool WithTransaction { get; set; }

        public bool RecoveryModeSimple { get; set; }

        public bool Debug { get; set; }

        public bool DryRun { get; set; }

        public bool Baseline { get; set; }

        public bool RunAllAnyTimeScripts { get; set; }

        #endregion

        public void run_the_task()
        {
            Log4NetAppender.configure_without_console();
            ApplicationConfiguraton.set_defaults_if_properties_are_not_set(this);


            if (Restore && string.IsNullOrEmpty(RestoreFromPath))
            {
                throw new Exception(
                    "If you set Restore to true, you must specify a location for the database to be restored from. That property is RestoreFromPath in MSBuild and restoreFromPath in NAnt.");
            }

            ApplicationConfiguraton.build_the_container(this);

            IRunner roundhouse_runner = new RoundhouseMigrationRunner(
                RepositoryPath,
                Container.get_an_instance_of<Environment>(),
                Container.get_an_instance_of<KnownFolders>(),
                Container.get_an_instance_of<FileSystemAccess>(),
                Container.get_an_instance_of<DatabaseMigrator>(),
                Container.get_an_instance_of<VersionResolver>(),
                Silent,
                Drop,
                DoNotCreateDatabase,
                WithTransaction,
                RecoveryModeSimple);

            roundhouse_runner.run();
        }

    }
}