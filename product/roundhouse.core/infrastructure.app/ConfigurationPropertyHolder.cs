using System.Collections.Generic;
using roundhouse.infrastructure.logging;
// ReSharper disable InconsistentNaming

namespace roundhouse.infrastructure.app
{
    using System;
    using databases;

    public interface ConfigurationPropertyHolder
    {
        Logger Logger { get; set; }
        string ServerName { get; set; }
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }
        string ConnectionStringAdmin { get; set; }
        int CommandTimeout { get; set; }
        int CommandTimeoutAdmin { get; set; }
        string SqlFilesDirectory { get; set; }
        string RepositoryPath { get; set; }
        string Version { get; set; }
        string VersionFile { get; set; }
        string VersionXPath { get; set; }
        string AlterDatabaseFolderName { get; set; }
        string RunAfterCreateDatabaseFolderName { get; set; }
        string RunBeforeUpFolderName { get; set; }
        string UpFolderName { get; set; }
        string DownFolderName { get; set; }
        string RunFirstAfterUpFolderName { get; set; }
        string FunctionsFolderName { get; set; }
        string ViewsFolderName { get; set; }
        string SprocsFolderName { get; set; }
        string TriggersFolderName { get; set; }
        string IndexesFolderName { get; set; }
        string RunAfterOtherAnyTimeScriptsFolderName { get; set; }
        string PermissionsFolderName { get; set; }
        string BeforeMigrationFolderName { get; set; }
        string AfterMigrationFolderName { get; set; }
        string SchemaName { get; set; }
        string VersionTableName { get; set; }
        string ScriptsRunTableName { get; set; }
        string ScriptsRunErrorsTableName { get; set; }
        bool DoNotStoreScriptsRunText { get; set; }
        [Obsolete("Use EnvironmentNames")]
        string EnvironmentName { get; set; }
        IList<string> EnvironmentNames { get; }
        bool Restore { get; set; }
        string RestoreFromPath { get; set; }
        string RestoreCustomOptions { get; set; }
        int RestoreTimeout { get; set; }
        string CreateDatabaseCustomScript { get; set; }
        string OutputPath { get; set; }
        bool WarnOnOneTimeScriptChanges { get; set; }
        bool WarnAndIgnoreOnOneTimeScriptChanges { get; set; }
        bool Silent { get; set; }
        string DatabaseType { get; set; }
        bool Drop { get; set; }
        bool DoNotCreateDatabase { get; set; }
        bool DoNotAlterDatabase { get; set; }
        bool WithTransaction { get; set; }
        RecoveryMode RecoveryMode { get; set; }
        bool Debug { get; set; }
        bool DryRun { get; set; }
        bool Baseline { get; set; }
        bool RunAllAnyTimeScripts { get; set; }
        bool DisableTokenReplacement { get; set; }
        bool SearchAllSubdirectoriesInsteadOfTraverse { get; set; }
        bool DisableOutput { get; set; }
        Dictionary<string, string> UserTokens { get; set; }
        System.Text.Encoding DefaultEncoding { get; set; }
        string ConfigurationFile { get; set; }
        IDictionary<string, string> to_token_dictionary();
	}
}