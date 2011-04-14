namespace roundhouse.infrastructure.app
{
    using log4net;
    using Microsoft.Build.Framework;

    public interface ConfigurationPropertyHolder
    {
        ITask MSBuildTask { get; }
        ILog Log4NetLogger { get; }

        string ServerName { get; set; }
        string DatabaseName { get; set; }
        string ConnectionString { get; set; }
        string ConnectionStringAdmin { get; set; }
        string SqlFilesDirectory { get; set; }
        string RepositoryPath { get; set; }
        string VersionFile { get; set; }
        string VersionXPath { get; set; }
        string UpFolderName { get; set; }
        string DownFolderName { get; set; }
        string RunFirstAfterUpFolderName { get; set; }
        string FunctionsFolderName { get; set; }
        string ViewsFolderName { get; set; }
        string SprocsFolderName { get; set; }
        string PermissionsFolderName { get; set; }
        string SchemaName { get; set; }
        string VersionTableName { get; set; }
        string ScriptsRunTableName { get; set; }
        string ScriptsRunErrorsTableName { get; set; }
        string EnvironmentName { get; set; }
        bool Restore { get; set; }
        string RestoreFromPath { get; set; }
        string RestoreCustomOptions { get; set; }
        int RestoreTimeout { get; set; }
        string CreateDatabaseCustomScript { get; set; }
        string OutputPath { get; set; }
        bool WarnOnOneTimeScriptChanges { get; set; }
        bool Silent { get; set; }
        string DatabaseType { get; set; }
        bool Drop { get; set; }
        bool DoNotCreateDatabase { get; set; }
        bool WithTransaction { get; set; }
        bool RecoveryModeSimple { get; set; }
        bool Debug { get; set; }
        bool DryRun { get; set; }
        bool Baseline { get; set; }
        bool RunAllAnyTimeScripts { get; set; }
    }
}