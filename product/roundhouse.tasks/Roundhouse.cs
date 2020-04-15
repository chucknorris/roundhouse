
namespace roundhouse.tasks
{
    using System;
    using System.Collections.Generic;
    using databases;
    using folders;
    using infrastructure.app;
    using infrastructure.app.logging;
    using infrastructure.containers;
    using infrastructure.extensions;
    using infrastructure.filesystem;
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using migrators;
    using environments;
    using resolvers;
    using runners;
    using Logger = roundhouse.infrastructure.logging.Logger;
    using System.Linq;

    public sealed class Roundhouse : ITask, ConfigurationPropertyHolder
    {
        private RecoveryMode recoveryMode;

        private readonly TaskLoggingHelper loggingHelper;

        public Roundhouse()
        {
            this.loggingHelper = new TaskLoggingHelper(this);
            EnvironmentNames = new List<string>();
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

        public string AccessToken { get; set; }

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

        public string TriggersFolderName { get; set; }

        public string IndexesFolderName { get; set; }

        public string RunAfterOtherAnyTimeScriptsFolderName { get; set; }

        public string PermissionsFolderName { get; set; }

        public string BeforeMigrationFolderName { get; set; }

        public string AfterMigrationFolderName { get; set; }

        public string SchemaName { get; set; }

        public string VersionTableName { get; set; }

        public string ScriptsRunTableName { get; set; }

        public string ScriptsRunErrorsTableName { get; set; }

        public bool DoNotStoreScriptsRunText { get; set; }

        [Obsolete("Use EnvironmentNames")]
        public string EnvironmentName {
            get { return EnvironmentNames.SingleOrDefault(); }
            set
            {
                EnvironmentNames.Clear();
                EnvironmentNames.Add(value);
            }
        }
        public IList<string> EnvironmentNames { get; private set; }

        public bool Restore { get; set; }

        public string RestoreFromPath { get; set; }

        public string RestoreCustomOptions { get; set; }

        public int RestoreTimeout { get; set; }

        public string CreateDatabaseCustomScript { get; set; }

        public bool Drop { get; set; }

        public bool DoNotCreateDatabase { get; set; }

        public bool DoNotAlterDatabase { get; set; }

        public string OutputPath { get; set; }

        public bool WarnOnOneTimeScriptChanges { get; set; }

        public bool WarnAndIgnoreOnOneTimeScriptChanges { get; set; }

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

        public System.Text.Encoding DefaultEncoding { get; set; }

        public bool DisableOutput { get; set; }
        public Dictionary<string, string> UserTokens { get; set; }

        public bool Initialize { get; set; }
        public string ConfigurationFile { get; set; }

        #endregion

        public IDictionary<string, string> to_token_dictionary()
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { nameof(AccessToken), AccessToken.to_string() },
                { nameof(AfterMigrationFolderName), AfterMigrationFolderName.to_string() },
                { nameof(AlterDatabaseFolderName), AlterDatabaseFolderName.to_string() },
                { nameof(Baseline), Baseline.to_string() },
                { nameof(BeforeMigrationFolderName), BeforeMigrationFolderName.to_string() },
                { nameof(CommandTimeout), CommandTimeout.to_string() },
                { nameof(CommandTimeoutAdmin), CommandTimeoutAdmin.to_string() },
                { nameof(ConfigurationFile), ConfigurationFile.to_string() },
                { nameof(ConnectionString), ConnectionString.to_string() },
                { nameof(ConnectionStringAdmin), ConnectionStringAdmin.to_string() },
                { nameof(CreateDatabaseCustomScript), CreateDatabaseCustomScript.to_string() },
                { nameof(DatabaseName), DatabaseName.to_string() },
                { nameof(DatabaseType), DatabaseType.to_string() },
                { nameof(Debug), Debug.to_string() },
                { nameof(DisableOutput), DisableOutput.to_string() },
                { nameof(DisableTokenReplacement), DisableTokenReplacement.to_string() },
                { nameof(DoNotAlterDatabase), DoNotAlterDatabase.to_string() },
                { nameof(DoNotCreateDatabase), DoNotCreateDatabase.to_string() },
                { nameof(DoNotStoreScriptsRunText), DoNotStoreScriptsRunText.to_string() },
                { nameof(DownFolderName), DownFolderName.to_string() },
                { nameof(Drop), Drop.to_string() },
                { nameof(DryRun), DryRun.to_string() },
#pragma warning disable 618
                { nameof(EnvironmentName), string.Join(",", EnvironmentNames) },
#pragma warning restore 618
                { nameof(EnvironmentNames), string.Join(",", EnvironmentNames) },
                { nameof(FunctionsFolderName), FunctionsFolderName.to_string() },
                { nameof(IndexesFolderName), IndexesFolderName.to_string() },
                { nameof(Initialize), Initialize.to_string() },
                { nameof(OutputPath), OutputPath.to_string() },
                { nameof(PermissionsFolderName), PermissionsFolderName.to_string() },
                { nameof(RecoveryMode), RecoveryMode.to_string() },
                { nameof(RepositoryPath), RepositoryPath.to_string() },
                { nameof(Restore), Restore.to_string() },
                { nameof(RestoreCustomOptions), RestoreCustomOptions.to_string() },
                { nameof(RestoreFromPath), RestoreFromPath.to_string() },
                { nameof(RestoreTimeout), RestoreTimeout.to_string() },
                { nameof(RunAfterCreateDatabaseFolderName), RunAfterCreateDatabaseFolderName.to_string() },
                { nameof(RunAfterOtherAnyTimeScriptsFolderName), RunAfterOtherAnyTimeScriptsFolderName.to_string() },
                { nameof(RunAllAnyTimeScripts), RunAllAnyTimeScripts.to_string() },
                { nameof(RunBeforeUpFolderName), RunBeforeUpFolderName.to_string() },
                { nameof(RunFirstAfterUpFolderName), RunFirstAfterUpFolderName.to_string() },
                { nameof(SchemaName), SchemaName.to_string() },
                { nameof(ScriptsRunErrorsTableName), ScriptsRunErrorsTableName.to_string() },
                { nameof(ScriptsRunTableName), ScriptsRunTableName.to_string() },
                { nameof(SearchAllSubdirectoriesInsteadOfTraverse), SearchAllSubdirectoriesInsteadOfTraverse.to_string() },
                { nameof(ServerName), ServerName.to_string() },
                { nameof(Silent), Silent.to_string() },
                { nameof(SprocsFolderName), SprocsFolderName.to_string() },
                { nameof(SqlFilesDirectory), SqlFilesDirectory.to_string() },
                { nameof(TriggersFolderName), TriggersFolderName.to_string() },
                { nameof(UpFolderName), UpFolderName.to_string() },
                { nameof(Version), Version.to_string() },
                { nameof(VersionFile), VersionFile.to_string() },
                { nameof(VersionTableName), VersionTableName.to_string() },
                { nameof(VersionXPath), VersionXPath.to_string() },
                { nameof(ViewsFolderName), ViewsFolderName.to_string() },
                { nameof(WarnAndIgnoreOnOneTimeScriptChanges), WarnAndIgnoreOnOneTimeScriptChanges.to_string() },
                { nameof(WarnOnOneTimeScriptChanges), WarnOnOneTimeScriptChanges.to_string() },
                { nameof(WithTransaction), WithTransaction.to_string() },
            };

            if (UserTokens != null)
            {
                foreach (var t in UserTokens)
                {
                    tokens[t.Key] = t.Value;
                }
            }

            return tokens;
        }

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
                Container.get_an_instance_of<EnvironmentSet>(),
                Container.get_an_instance_of<KnownFolders>(),
                Container.get_an_instance_of<FileSystemAccess>(),
                Container.get_an_instance_of<DatabaseMigrator>(),
                Container.get_an_instance_of<VersionResolver>(),
                Silent,
                Drop,
                DoNotCreateDatabase,
                WithTransaction,
                this);

            roundhouse_runner.run();
        }
    }
}