namespace roundhouse.consoles
{
    using System;
    using databases;
    using infrastructure.app;
    using infrastructure.logging;

    public sealed class DefaultConfiguration : ConfigurationPropertyHolder
    {
        public Logger Logger { get; set; }
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string ConnectionString { get; set; }
        public string ConnectionStringAdmin { get; set; }
        public int CommandTimeout { get; set; }
        public int CommandTimeoutAdmin { get; set; }
        public string SqlFilesDirectory { get; set; }
        public string RepositoryPath { get; set; }
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
        public string OutputPath { get; set; }
        public bool WarnOnOneTimeScriptChanges { get; set; }
        public bool Silent { get; set; }
        public string DatabaseType { get; set; }
        public bool Drop { get; set; }
        public bool DoNotCreateDatabase { get; set; }
        public bool WithTransaction { get; set; }
        public RecoveryMode RecoveryMode { get; set; }
        [Obsolete("Use RecoveryMode = Simple")]
        public bool RecoveryModeSimple { get; set; }
        public bool Debug { get; set; }
        public bool DryRun { get; set; }
        public bool Baseline { get; set; }
        public bool RunAllAnyTimeScripts { get; set; }
        public bool DisableTokenReplacement { get; set; }
        public bool SearchAllSubdirectoriesInsteadOfTraverse { get; set; }
        public bool DisableOutput { get; set; }
    }
}