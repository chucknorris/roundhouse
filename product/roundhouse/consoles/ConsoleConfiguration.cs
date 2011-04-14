namespace roundhouse.consoles
{
    using infrastructure.app;
    using log4net;
    using Microsoft.Build.Framework;

    public sealed class ConsoleConfiguration : ConfigurationPropertyHolder
    {
        public ConsoleConfiguration(ILog log4net_logger)
        {
            Log4NetLogger = log4net_logger;
        }

        public ITask MSBuildTask
        {
            get { return null; }
        }

        public ILog Log4NetLogger { get; private set; }
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
        public string OutputPath { get; set; }
        public bool WarnOnOneTimeScriptChanges { get; set; }
        public bool Silent { get; set; }
        public string DatabaseType { get; set; }
        public bool Drop { get; set; }
        public bool DoNotCreateDatabase { get; set; }
        public bool WithTransaction { get; set; }
        public bool RecoveryModeSimple { get; set; }
        public bool Debug { get; set; }
        public bool DryRun { get; set; }
        public bool Baseline { get; set; }
        public bool RunAllAnyTimeScripts { get; set; }
    }
}