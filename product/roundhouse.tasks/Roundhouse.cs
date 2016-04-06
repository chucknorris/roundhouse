namespace roundhouse.tasks
{
	using databases;
	using folders;
	using infrastructure.app;
	using infrastructure.app.logging;
	using infrastructure.containers;
	using infrastructure.filesystem;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using migrators;
	using resolvers;
	using runners;
	using System;
	using System.Collections.Generic;
	using Environment = environments.Environment;
	using Logger = roundhouse.infrastructure.logging.Logger;

    public sealed class Roundhouse : ITask, ConfigurationPropertyHolder
    {
        private RecoveryMode recoveryMode;

        private readonly TaskLoggingHelper loggingHelper;

        public Roundhouse()
        {
            this.loggingHelper = new TaskLoggingHelper(this);
        }

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
            return !this.loggingHelper.HasLoggedErrors;
        }

        #endregion

        #region Properties

        public Logger Logger { get; set; }

        public string ServerName { get; set; }

        public string DatabaseName { get; set; }

        public string ConnectionString { get; set; }

        public string ConnectionStringAdmin { get; set; }

        public int CommandTimeout { get; set; }

        public int CommandTimeoutAdmin { get; set; }

        public string SqlFilesDirectory { get; set; }

        public string RepositoryPath { get; set; }

        public string Version { get; set; }

        public string VersionFile { get; set; }

        public string VersionXPath { get; set; }

        public string AlterDatabaseFolderName { get; set; }

        public string RunAfterCreateDatabaseFolderName { get; set; }

		public string RunBeforeUpFolderName { get; set; }

        public string UpFolderName { get; set; }

        public string DownFolderName { get; set; }

        public string RunFirstAfterUpFolderName { get; set; }

        public string FunctionsFolderName { get; set; }

        public string ViewsFolderName { get; set; }

        public string SprocsFolderName { get; set; }

        public string IndexesFolderName { get; set; }

        public string RunAfterOtherAnyTimeScriptsFolderName { get; set; }

        public string PermissionsFolderName { get; set; }

        public string BeforeMigrationFolderName { get; set; }

        public string AfterMigrationFolderName { get; set; }

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

        RecoveryMode ConfigurationPropertyHolder.RecoveryMode
        {
            get
            {
                return this.recoveryMode;
            }
            set
            {
                this.recoveryMode = value;
            }
        }

        public string RecoveryMode
        {
            get
            {
                return this.recoveryMode.ToString();
            }
            set
            {
                RecoveryMode result;
                if (Enum.TryParse(value, true, out result))
                {
                    this.recoveryMode = result;
                }
                else
                {
                    this.loggingHelper.LogError("The value of 'RecoveryMode' must be one of these values: 'NoChange', 'Simple' or 'Full'. Actual value was '{0}'.", value);
                }
            }
        }

        [Obsolete("Use RecoverMode=Simple now")]
        public bool RecoveryModeSimple { get; set; }

        public bool Debug { get; set; }

        public bool DryRun { get; set; }

        public bool Baseline { get; set; }

        public bool RunAllAnyTimeScripts { get; set; }

        public bool DisableTokenReplacement { get; set; }

        public bool SearchAllSubdirectoriesInsteadOfTraverse { get; set; }

        public bool DisableOutput { get; set; }

        public Dictionary<string, string> UserTokens { get; set; }

        public bool Initialize { get; set; }

        #endregion

        public void run_the_task()
        {
            if (this.loggingHelper.HasLoggedErrors)
            {
                return;
            }

            Log4NetAppender.configure_without_console();
            ApplicationConfiguraton.set_defaults_if_properties_are_not_set(this);


            if (Restore && string.IsNullOrEmpty(RestoreFromPath))
            {
                throw new Exception(
                    "If you set Restore to true, you must specify a location for the database to be restored from. That property is RestoreFromPath in MSBuild.");
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
                RecoveryModeSimple,
                this);

            roundhouse_runner.run();
        }
    }
}