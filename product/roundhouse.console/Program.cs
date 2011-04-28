namespace roundhouse.console
{
    using System;
    using consoles;
    using folders;
    using infrastructure;
    using infrastructure.app;
    using infrastructure.containers;
    using infrastructure.filesystem;
    using log4net;
    using migrators;
    using resolvers;
    using runners;
    using infrastructure.commandline.options;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using log4net.Core;
    using log4net.Repository;
    using infrastructure.app.logging;
    using infrastructure.extensions;

    internal class Program
    {
        private static readonly ILog the_logger = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            Log4NetAppender.configure();
            
            try
            {
                // determine if this a call to the diff or the migrator
                if (string.Join("|", args).to_lower().Contains("version") && args.Length == 1)
                {
                    report_version();
                }
                else if (string.Join("|", args).to_lower().Contains("rh.redgate.diff"))
                {
                    run_diff_utility(set_up_configuration_and_build_the_container(args));
                }
                else
                {
                    run_migrator(set_up_configuration_and_build_the_container(args));
                }

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                the_logger.Info(ex.Message);
                Environment.Exit(1);
            }


        }

        public static void report_version()
        {
            string version = infrastructure.VersionInformation.get_current_assembly_version();
            the_logger.InfoFormat("{0} - version {1} from http://projectroundhouse.org.", ApplicationParameters.name, version);

        }

        public static ConfigurationPropertyHolder set_up_configuration_and_build_the_container(string[] args)
        {
            ConfigurationPropertyHolder configuration = new ConsoleConfiguration(the_logger);
            parse_arguments_and_set_up_configuration(configuration, args);
            if (configuration.Debug)
            {
                change_log_to_debug_level();
            }

            ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
            ApplicationConfiguraton.build_the_container(configuration);

            return configuration;
        }

        private static void parse_arguments_and_set_up_configuration(ConfigurationPropertyHolder configuration, string[] args)
        {
            bool help = false;

            OptionSet option_set = new OptionSet()
                .Add("?|help|h",
                    "Prints out the options.",
                    option => help = option != null)
                .Add("d=|db=|database=|databasename=",
                    "REQUIRED: DatabaseName - The database you want to create/migrate.",
                    option => configuration.DatabaseName = option)
                .Add("c=|cs=|connstring=|connectionstring=",
                    string.Format("REQUIRED: ConnectionString - As an alternative to ServerName and Database - You can provide an entire connection string instead."),
                    option => configuration.ConnectionString = option)
                .Add("f=|files=|sqlfilesdirectory=",
                    string.Format("SqlFilesDirectory - The directory where your SQL scripts are. Defaults to \"{0}\".",
                        ApplicationParameters.default_files_directory),
                    option => configuration.SqlFilesDirectory = option)
                .Add("s=|server=|servername=|instance=|instancename=",
                    string.Format("ServerName - The server and instance you would like to run on. (local) and (local)\\SQL2008 are both valid values. Defaults to \"{0}\".",
                        ApplicationParameters.default_server_name),
                    option => configuration.ServerName = option)
                .Add("csa=|connstringadmin=|connectionstringadministration=",
                    string.Format("ConnectionStringAdministration - This is used for connecting to master when you may have a different uid and password than normal."),
                    option => configuration.ConnectionStringAdmin = option)
                //database type
                .Add("dt=|dbt=|databasetype=",
                    string.Format("DatabaseType - Tells RH what type of database it is running on. This is a plugin model. This is the fully qualified name of a class that implements the interface roundhouse.sql.Database, roundhouse. If you have your own assembly, just set it next to rh.exe and set this value appropriately. Defaults to \"{0}\" which can also run against SQL Server 2005.",
                        ApplicationParameters.default_database_type),
                    option => configuration.DatabaseType = option)
                // versioning
                .Add("r=|repo=|repositorypath=",
                    string.Format("RepositoryPath - The repository. A string that can be anything. Used to track versioning along with the version. Defaults to null."),
                    option => configuration.RepositoryPath = option)
                .Add("vf=|versionfile=",
                    string.Format("VersionFile - Either an XML file or a DLL that a version can be resolved from. Defaults to \"{0}\".",
                        ApplicationParameters.default_version_file),
                    option => configuration.VersionFile = option)
                .Add("vx=|versionxpath=",
                    string.Format("VersionXPath - Works in conjunction with an XML version file. Defaults to \"{0}\".",
                        ApplicationParameters.default_version_x_path),
                    option => configuration.VersionXPath = option)
                // folders
                .Add("u=|up=|upfolder=|upfoldername=",
                    string.Format("UpFolderName - The name of the folder where you keep your update scripts. Will recurse through subfolders. Defaults to \"{0}\".",
                        ApplicationParameters.default_up_folder_name),
                    option => configuration.UpFolderName = option)
                .Add("do=|down=|downfolder=|downfoldername=",
                    string.Format("DownFolderName - The name of the folder where you keep your versioning down scripts. Will recurse through subfolders. Defaults to \"{0}\".",
                        ApplicationParameters.default_down_folder_name),
                    option => configuration.DownFolderName = option)
                .Add("rf=|runfirst=|runfirstfolder=|runfirstafterupdatefolder=|runfirstafterupdatefoldername=",
                    string.Format("RunFirstAfterUpdateFolderName - The name of the folder where you keep any functions, views, or sprocs that are order dependent. If you have a function that depends on a view, you definitely need the view in this folder. Will recurse through subfolders. Defaults to \"{0}\".",
                        ApplicationParameters.default_run_first_after_up_folder_name),
                    option => configuration.RunFirstAfterUpFolderName = option)
                .Add("fu=|functions=|functionsfolder=|functionsfoldername=",
                    string.Format("FunctionsFolderName - The name of the folder where you keep your functions. Will recurse through subfolders. Defaults to \"{0}\".",
                        ApplicationParameters.default_functions_folder_name),
                    option => configuration.FunctionsFolderName = option)
                .Add("vw=|views=|viewsfolder=|viewsfoldername=",
                    string.Format("ViewsFolderName - The name of the folder where you keep your views. Will recurse through subfolders. Defaults to \"{0}\".",
                        ApplicationParameters.default_views_folder_name),
                    option => configuration.ViewsFolderName = option)
                .Add("sp=|sprocs=|sprocsfolder=|sprocsfoldername=",
                    string.Format("SprocsFolderName - The name of the folder where you keep your stored procedures. Will recurse through subfolders. Defaults to \"{0}\".",
                        ApplicationParameters.default_sprocs_folder_name),
                    option => configuration.SprocsFolderName = option)
                .Add("ra=|runAfterOtherAnyTimeScripts=|runAfterOtherAnyTimeScriptsfolder=|runAfterOtherAnyTimeScriptsfoldername=",
                    string.Format("RunAfterOtherAnyTimeScriptsFolderName - The name of the folder where you keep scripts that will be run after all of the other any time scripts complete. Will recurse through subfolders. Defaults to \"{0}\".",
                        ApplicationParameters.default_runAfterOtherAnyTime_folder_name),
                    option => configuration.RunAfterOtherAnyTimeScriptsFolderName = option)
                .Add("p=|permissions=|permissionsfolder=|permissionsfoldername=",
                    string.Format("PermissionsFolderName - The name of the folder where you keep your permissions scripts. Will recurse through subfolders. Defaults to \"{0}\".",
                        ApplicationParameters.default_permissions_folder_name),
                    option => configuration.PermissionsFolderName = option)
                // roundhouse items
                .Add("sc=|schema=|schemaname=",
                    string.Format("SchemaName - This is the schema where RH stores it's tables. Once you set this a certain way, do not change this. This is definitely running with scissors and very sharp. I am allowing you to have flexibility, but because this is a knife you can still get cut if you use it wrong. I'm just saying. You've been warned. Defaults to \"{0}\".",
                        ApplicationParameters.default_roundhouse_schema_name),
                    option => configuration.SchemaName = option)
                .Add("vt=|versiontable=|versiontablename=",
                    string.Format("VersionTableName - This is the table where RH stores versioning information. Once you set this, do not change this. This is definitely running with scissors and very sharp. Defaults to \"{0}\".",
                        ApplicationParameters.default_version_table_name),
                    option => configuration.VersionTableName = option)
                .Add("srt=|scriptsruntable=|scriptsruntablename=",
                    string.Format("ScriptsRunTableName - This is the table where RH stores information about scripts that have been run. Once you set this a certain way, do not change this. This is definitely running with scissors and very sharp. Defaults to \"{0}\".",
                        ApplicationParameters.default_scripts_run_table_name),
                    option => configuration.ScriptsRunTableName = option)
                .Add("sret=|scriptsrunerrorstable=|scriptsrunerrorstablename=",
                    string.Format("ScriptsRunErrorsTableName - This is the table where RH stores information about scripts that have been run with errors. Once you set this a certain way, do not change this. This is definitelly running with scissors and very sharp. Defaults to \"{0}\".",
                        ApplicationParameters.default_scripts_run_errors_table_name),
                    option => configuration.ScriptsRunErrorsTableName = option)
                //environment
                .Add("env=|environment=|environmentname=",
                    string.Format("EnvironmentName - This allows RH to be environment aware and only run scripts that are in a particular environment based on the naming of the script. LOCAL.something.sql would only be run in the LOCAL environment. Defaults to \"{0}\".",
                        ApplicationParameters.default_environment_name),
                    option => configuration.EnvironmentName = option)
                //restore
                .Add("restore",
                    "Restore - This instructs RH to do a restore (with the restorefrompath parameter) of a database before running migration scripts. Defaults to false.",
                    option => configuration.Restore = option != null)
                .Add("rfp=|restorefrom=|restorefrompath=",
                    "RestoreFromPath - This tells the restore where to get to the backed up database. Defaults to null. Required if /restore has been set. NOTE: will try to use Litespeed for the restore if the last two characters of the name are LS (as in DudeLS.bak).",
                    option => configuration.RestoreFromPath = option)
                .Add("rco=|restoreoptions=|restorecustomoptions=",
                    "RestoreCustomOptions - This provides the restore any custom options as in MOVE='Somewhere or another'.",
                    option => configuration.RestoreCustomOptions = option)
                .Add("rt=|restoretimeout=",
                    "RestoreTimeout - Allows you to specify a restore timeout in seconds. The default is 900 seconds.",
                    option => configuration.RestoreTimeout = int.Parse(option))
                //custom create database
                .Add("cds=|createdatabasescript=|createdatabasecustomscript=",
                    "CreateDatabaseCustomScript - This instructs RH to use this script for creating a database instead of the default based on the SQLType.",
                    option => configuration.CreateDatabaseCustomScript = option)
                //drop
                .Add("drop",
                    "Drop - This instructs RH to remove a database and not run migration scripts. Defaults to false.",
                    option => configuration.Drop = option != null)
                //don't create the database if it doesn't exist
                .Add("dc|dnc|donotcreatedatabase",
                    "DoNotCreateDatabase - This instructs RH to not create a database if it does not exists. Defaults to false.",
                    option => configuration.DoNotCreateDatabase = option != null)
                //output
                .Add("o=|output=|outputpath=",
                    string.Format("OutputPath - This is where everything related to the migration is stored. This includes any backups, all items that ran, permission dumps, logs, etc. Defaults to \"{0}\".",
                        ApplicationParameters.default_output_path),
                    option => configuration.OutputPath = option)
                //warn on changes
                .Add("w|warnononetimescriptchanges",
                    "WarnOnOneTimeScriptChanges - If you do not want RH to error when you change scripts that should not change, you must set this flag. One time scripts are DDL/DML (anything in the upFolder). Defaults to false.",
                    option => configuration.WarnOnOneTimeScriptChanges = option != null)
                //silent?
                .Add("silent|ni|noninteractive",
                    "Silent - tells RH not to ask for any input when it runs. Defaults to false.",
                    option => configuration.Silent = option != null)
                //transaction
                .Add("t|trx|transaction|wt|withtransaction",
                    "WithTransaction - This instructs RH to run inside of a transaction. Defaults to false.",
                    option => configuration.WithTransaction = option != null)
                //simple
                .Add("simple",
                    "RecoveryModeSimple - This instructs RH to set the database recovery mode to simple recovery. Defaults to false.",
                    option => configuration.RecoveryModeSimple = option != null)
                //debug
                .Add("debug",
                    "Debug - This instructs RH to write out all messages.",
                    option => configuration.Debug = option != null)
                //force all anytime scripts
                .Add("runallanytimescripts|forceanytimescripts",
                    "RunAllAnyTimeScripts - This instructs RH to run any time scripts every time it is run.",
                    option => configuration.RunAllAnyTimeScripts = option != null)
                //recorders
                .Add("baseline",
                    "Baseline - This instructs RH to create an insert for its recording tables, but not to actually run anything against the database. Use this option if you already have scripts that have been run through other means (and BEFORE you start the new ones).",
                    option => configuration.Baseline = option != null)
                .Add("dryrun",
                    "DryRun - This instructs RH to log what would have run, but not to actually run anything against the database. Use this option if you are trying to figure out what RH is going to do.",
                    option => configuration.DryRun = option != null)
               ;

            try
            {
                option_set.Parse(args);
            }
            catch (OptionException)
            {
                show_help("Error, usage is:", option_set);
            }

            if (help)
            {
                the_logger.Info("Usage of RoundhousE (RH)");
                string usage_message =
                    string.Format(
                    "rh.exe /d[atabase] VALUE OR rh.exe /c[onnection]s[tring] VALUE followed by all the optional parameters {0}" +
                    "[" +
                    "/[sql]f[ilesdirectory] VALUE " +
                    "/s[ervername] VALUE " +
                    "/c[onnection]s[tring]a[dministration] VALUE " +
                    "/r[epositorypath] VALUE /v[ersion]f[ile] VALUE /v[ersion]x[path] VALUE " +
                    "/u[pfoldername] VALUE /do[wnfoldername] VALUE " +
                    "/r[un]f[irstafterupdatefoldername] VALUE /fu[nctionsfoldername] VALUE /v[ie]w[sfoldername] VALUE " +
                    "/sp[rocsfoldername] VALUE /p[ermissionsfoldername] VALUE " +
                    "/sc[hemaname] VALUE /v[ersion]t[ablename] VALUE /s[cripts]r[un]t[ablename] VALUE /s[cripts]r[un]e[rrors]t[ablename] VALUE " +
                    "/env[ironmentname] VALUE " +
                    "/restore /r[estore]f[rom]p[ath] VALUE /r[estore]c[ustom]o[ptions] VALUE /r[estore]t[imeout] VALUE" +
                    "/c[reate]d[atabasecustom]s[cript] VALUE " +
                    "/env[ironmentname] VALUE " +
                    "/o[utputpath] VALUE " +
                    "/w[arnononetimescriptchanges] " +
                    "/silent " +
                    "/d[atabase]t[ype] VALUE " +
                    "/drop " +
                    "/d[onot]c[reatedatabase] " +
                    "/t[ransaction] " +
                    "/simple " +
                    "/debug " +
                    "/runallanytimescripts " +
                    "/baseline " +
                    "/dryrun " +
                    "]", Environment.NewLine);
                show_help(usage_message, option_set);
            }

            if (string.IsNullOrEmpty(configuration.DatabaseName) && string.IsNullOrEmpty(configuration.ConnectionString))
            {
                show_help("Error: You must specify Database Name (/d) OR Connection String (/cs) at a minimum to use RoundhousE.", option_set);
            }
        }

        public static void show_help(string message, OptionSet option_set)
        {
            //Console.Error.WriteLine(message);
            the_logger.Info(message);
            option_set.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }

        public static void change_log_to_debug_level()
        {
            ILoggerRepository log_repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
            log_repository.Threshold = Level.Debug;
        }

        public static void run_migrator(ConfigurationPropertyHolder configuration)
        {
            RoundhouseMigrationRunner migration_runner = get_migration_runner(configuration);
            migration_runner.run();

            if (!configuration.Silent)
            {
                Console.WriteLine("{0}Please press enter to continue...", Environment.NewLine);
                Console.Read();
            }
        }

        private static void run_diff_utility(ConfigurationPropertyHolder configuration)
        {
            bool silent = configuration.Silent;

            RoundhouseRedGateCompareRunner diff_runner = get_diff_runner(configuration, get_migration_runner(configuration));
            diff_runner.run();

            if (!silent)
            {
                Console.WriteLine("{0}Please press enter to continue...", Environment.NewLine);
                Console.Read();
            }
        }

        private static RoundhouseMigrationRunner get_migration_runner(ConfigurationPropertyHolder configuration)
        {
            return new RoundhouseMigrationRunner(
                 configuration.RepositoryPath,
                 Container.get_an_instance_of<environments.Environment>(),
                 Container.get_an_instance_of<KnownFolders>(),
                 Container.get_an_instance_of<FileSystemAccess>(),
                 Container.get_an_instance_of<DatabaseMigrator>(),
                 Container.get_an_instance_of<VersionResolver>(),
                 configuration.Silent,
                 configuration.Drop,
                 configuration.DoNotCreateDatabase,
                 configuration.WithTransaction,
                 configuration.RecoveryModeSimple);
        }

        private static RoundhouseRedGateCompareRunner get_diff_runner(ConfigurationPropertyHolder configuration, RoundhouseMigrationRunner migration_runner)
        {
            return new RoundhouseRedGateCompareRunner(
                  Container.get_an_instance_of<KnownFolders>(),
                  Container.get_an_instance_of<FileSystemAccess>(),
                  configuration, migration_runner);
        }

    }
}