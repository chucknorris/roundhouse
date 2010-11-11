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
    using NAnt.Core;
    using NAnt.Core.Attributes;
    using resolvers;
    using runners;
    using Environment = environments.Environment;

    [TaskName("roundhouse")]
    public sealed class Roundhouse : Task, ITask, ConfigurationPropertyHolder
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

        #region NAnt

        /// <summary>
        /// Executes the NAnt task
        /// </summary>
        protected override void ExecuteTask()
        {
            if (Debug)
            {
                Project.Threshold = Level.Debug;
            }

            run_the_task();
        }

        #endregion

        #region Properties

        public ILog Log4NetLogger
        {
            get { return the_logger; }
        }

        public Task NAntTask
        {
            get { return this; }
        }

        public ITask MSBuildTask
        {
            get { return this; }
        }


        [TaskAttribute("serverName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ServerName { get; set; }

        [Required]
        [TaskAttribute("databaseName", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string DatabaseName { get; set; }

        [TaskAttribute("connectionString", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ConnectionString { get; set; }
        
        [TaskAttribute("connectionStringAdmin", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ConnectionStringAdmin { get; set; }

        [Required]
        [TaskAttribute("sqlFilesDirectory", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string SqlFilesDirectory { get; set; }

        [TaskAttribute("repositoryPath", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string RepositoryPath { get; set; }

        [TaskAttribute("versionFile", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string VersionFile { get; set; }

        [TaskAttribute("versionXPath", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string VersionXPath { get; set; }

        [TaskAttribute("upFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string UpFolderName { get; set; }

        [TaskAttribute("downFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string DownFolderName { get; set; }

        [TaskAttribute("runFirstAfterUpFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string RunFirstAfterUpFolderName { get; set; }

        [TaskAttribute("functionsFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string FunctionsFolderName { get; set; }

        [TaskAttribute("viewsFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ViewsFolderName { get; set; }

        [TaskAttribute("sprocsFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string SprocsFolderName { get; set; }

        [TaskAttribute("permissionsFolderName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string PermissionsFolderName { get; set; }

        [TaskAttribute("schemaName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string SchemaName { get; set; }

        [TaskAttribute("versionTableName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string VersionTableName { get; set; }

        [TaskAttribute("scriptsRunTableName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ScriptsRunTableName { get; set; }

        [TaskAttribute("scriptsRunErrorsTableName", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string ScriptsRunErrorsTableName { get; set; }

        [TaskAttribute("environmentName", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string EnvironmentName { get; set; }

        [TaskAttribute("restore", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool Restore { get; set; }

        [TaskAttribute("restoreFromPath", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string RestoreFromPath { get; set; }

        [TaskAttribute("restoreCustomOptions", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string RestoreCustomOptions { get; set; }

        [TaskAttribute("restoreTimeout", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public int RestoreTimeout { get; set; }

        [TaskAttribute("createDatabaseCustomScript", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string CreateDatabaseCustomScript { get; set; }

        [TaskAttribute("drop", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool Drop { get; set; }

        [TaskAttribute("doNotCreateDatabase", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool DoNotCreateDatabase { get; set; }

        [TaskAttribute("outputPath", Required = false)]
        [StringValidator(AllowEmpty = true)]
        public string OutputPath { get; set; }

        [TaskAttribute("warnOnOneTimeScriptChanges", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool WarnOnOneTimeScriptChanges { get; set; }

        [TaskAttribute("silent", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool Silent { get; set; }

        [TaskAttribute("databaseType", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public string DatabaseType { get; set; }

        [TaskAttribute("withTransaction", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool WithTransaction { get; set; }

        [TaskAttribute("recoveryModeSimple", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool RecoveryModeSimple { get; set; }

        [TaskAttribute("debug", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool Debug { get; set; }
        
        [TaskAttribute("dryRun", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool DryRun { get; set; }  
        
        [TaskAttribute("baseline", Required = false)]
        [StringValidator(AllowEmpty = false)]
        public bool Baseline { get; set; }
        
        [TaskAttribute("runAllAnyTimeScripts", Required = false)]
        [StringValidator(AllowEmpty = false)]
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