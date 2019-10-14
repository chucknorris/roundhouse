using System.Collections.Generic;

namespace roundhouse.consoles
{
    using System;
    using databases;
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.logging;
    using System.Linq;

    public sealed class DefaultConfiguration : ConfigurationPropertyHolder
    {
        public DefaultConfiguration()
        {
            EnvironmentNames = new List<string>();
        }
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
        public string OutputPath { get; set; }
        public bool WarnOnOneTimeScriptChanges { get; set; }
        public bool WarnAndIgnoreOnOneTimeScriptChanges { get; set; }
        public bool Silent { get; set; }
        public string DatabaseType { get; set; }
        public bool Drop { get; set; }
        public bool DoNotCreateDatabase { get; set; }
        public bool DoNotAlterDatabase { get; set; }
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
        public Dictionary<string, string> UserTokens { get; set; }
        public bool Initialize { get; set; }
        public string ConfigurationFile { get; set; }
        public System.Text.Encoding DefaultEncoding { get; set; }

        public IDictionary<string, string> to_token_dictionary()
        {
            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            tokens["AfterMigrationFolderName"] = AfterMigrationFolderName.to_string();
            tokens["AlterDatabaseFolderName"] = AlterDatabaseFolderName.to_string();
            tokens["Baseline"] = Baseline.to_string();
            tokens["BeforeMigrationFolderName"] = BeforeMigrationFolderName.to_string();
            tokens["CommandTimeout"] = CommandTimeout.to_string();
            tokens["CommandTimeoutAdmin"] = CommandTimeoutAdmin.to_string();
            tokens["ConfigurationFile"] = ConfigurationFile.to_string();
            tokens["ConnectionString"] = ConnectionString.to_string();
            tokens["ConnectionStringAdmin"] = ConnectionStringAdmin.to_string();
            tokens["CreateDatabaseCustomScript"] = CreateDatabaseCustomScript.to_string();
            tokens["DatabaseName"] = DatabaseName.to_string();
            tokens["DatabaseType"] = DatabaseType.to_string();
            tokens["Debug"] = Debug.to_string();
            tokens["DisableOutput"] = DisableOutput.to_string();
            tokens["DisableTokenReplacement"] = DisableTokenReplacement.to_string();
            tokens["DoNotAlterDatabase"] = DoNotAlterDatabase.to_string();
            tokens["DoNotCreateDatabase"] = DoNotCreateDatabase.to_string();
            tokens["DoNotStoreScriptsRunText"] = DoNotStoreScriptsRunText.to_string();
            tokens["DownFolderName"] = DownFolderName.to_string();
            tokens["Drop"] = Drop.to_string();
            tokens["DryRun"] = DryRun.to_string();
            tokens["EnvironmentName"] = string.Join(",", EnvironmentNames);
            tokens["EnvironmentNames"] = string.Join(",", EnvironmentNames);
            tokens["FunctionsFolderName"] = FunctionsFolderName.to_string();
            tokens["IndexesFolderName"] = IndexesFolderName.to_string();
            tokens["Initialize"] = Initialize.to_string();
            tokens["OutputPath"] = OutputPath.to_string();
            tokens["PermissionsFolderName"] = PermissionsFolderName.to_string();
            tokens["RecoveryMode"] = RecoveryMode.to_string();
            tokens["RepositoryPath"] = RepositoryPath.to_string();
            tokens["Restore"] = Restore.to_string();
            tokens["RestoreCustomOptions"] = RestoreCustomOptions.to_string();
            tokens["RestoreFromPath"] = RestoreFromPath.to_string();
            tokens["RestoreTimeout"] = RestoreTimeout.to_string();
            tokens["RunAfterCreateDatabaseFolderName"] = RunAfterCreateDatabaseFolderName.to_string();
            tokens["RunAfterOtherAnyTimeScriptsFolderName"] = RunAfterOtherAnyTimeScriptsFolderName.to_string();
            tokens["RunAllAnyTimeScripts"] = RunAllAnyTimeScripts.to_string();
            tokens["RunBeforeUpFolderName"] = RunBeforeUpFolderName.to_string();
            tokens["RunFirstAfterUpFolderName"] = RunFirstAfterUpFolderName.to_string();
            tokens["SchemaName"] = SchemaName.to_string();
            tokens["ScriptsRunErrorsTableName"] = ScriptsRunErrorsTableName.to_string();
            tokens["ScriptsRunTableName"] = ScriptsRunTableName.to_string();
            tokens["SearchAllSubdirectoriesInsteadOfTraverse"] = SearchAllSubdirectoriesInsteadOfTraverse.to_string();
            tokens["ServerName"] = ServerName.to_string();
            tokens["Silent"] = Silent.to_string();
            tokens["SprocsFolderName"] = SprocsFolderName.to_string();
            tokens["SqlFilesDirectory"] = SqlFilesDirectory.to_string();
            tokens["TriggersFolderName"] = TriggersFolderName.to_string();
            tokens["UpFolderName"] = UpFolderName.to_string();
            tokens["Version"] = Version.to_string();
            tokens["VersionFile"] = VersionFile.to_string();
            tokens["VersionTableName"] = VersionTableName.to_string();
            tokens["VersionXPath"] = VersionXPath.to_string();
            tokens["ViewsFolderName"] = ViewsFolderName.to_string();
            tokens["WarnAndIgnoreOnOneTimeScriptChanges"] = WarnAndIgnoreOnOneTimeScriptChanges.to_string();
            tokens["WarnOnOneTimeScriptChanges"] = WarnOnOneTimeScriptChanges.to_string();
            tokens["WithTransaction"] = WithTransaction.to_string();

            if (UserTokens != null)
            {
                foreach (var t in UserTokens)
                {
                    tokens[t.Key] = t.Value;
                }
            }

            return tokens;
        }

    }
}