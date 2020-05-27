﻿using System;
using System.Linq;
using System.Net;
using log4net;
using roundhouse.consoles;
using roundhouse.databases;
using roundhouse.folders;
using roundhouse.infrastructure;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.app.tokens;
using roundhouse.infrastructure.app.logging;
using roundhouse.infrastructure.commandline.options;
using roundhouse.infrastructure.containers;
using roundhouse.infrastructure.extensions;
using roundhouse.infrastructure.filesystem;
using roundhouse.init;
using roundhouse.migrators;
using roundhouse.resolvers;
using roundhouse.runners;
using System.Reflection;
using log4net.Repository;
using Newtonsoft.Json;
using System.Collections.Generic;
using log4net.Core;
using System.Text;

namespace roundhouse.console
{
    public class Program
    {
        private static readonly char[] OptionsSplit = new[] { ',',';' };

        private static readonly ILog the_logger = LogManager.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            Log4NetAppender.configure();

            init_security_protocol();

            int exit_code = 0;

            try
            {
                // determine if this a call to the diff, the migrator, or the init
                if (string.Join("|", args).to_lower().Contains("version") && args.Length == 1)
                {
                    report_version();
                }
                else if (string.Join("|", args).to_lower().Contains("rh.redgate.diff"))
                {
                    run_diff_utility(set_up_configuration_and_build_the_container(args));
                }
                else if (string.Join("|", args).to_lower().Contains("isuptodate"))
                {
                    run_update_check(set_up_configuration_and_build_the_container(args));
                }
                else if (args.Any() && args[0] == "init")
                {
                    var cf = set_up_configuration_and_build_the_container(args, Mode.Init);
                    init_folder(cf);
                }
                else
                {
                    var cf = set_up_configuration_and_build_the_container(args);
                    run_migrator(cf);
                }
            }
          
            
            catch (Exception ex)
            {
                the_logger.Error(ex.Message, ex);
                exit_code = 1;
            }
            finally
            {
#if DEBUG
                System.Console.WriteLine("Press any key to continue...");
                System.Console.ReadKey();
#endif
                Environment.Exit(exit_code);
            }
        }

        public static void report_version()
        {
            string version = VersionInformation.get_current_assembly_version();
            the_logger.InfoFormat("{0} - version {1} from http://projectroundhouse.org.", ApplicationParameters.name, version);
        }

        public enum Mode
        {
            Normal,
            Init
        }

        public static ConfigurationPropertyHolder set_up_configuration_and_build_the_container(string[] args, Mode mode = Mode.Normal)
        {
            ConfigurationPropertyHolder configuration = new DefaultConfiguration();
            parse_arguments_and_set_up_configuration(configuration, args, mode);

            ApplicationConfiguraton.set_defaults_if_properties_are_not_set(configuration);
            ApplicationConfiguraton.build_the_container(configuration);

            return configuration;
        }

        private static void parse_arguments_and_set_up_configuration(ConfigurationPropertyHolder configuration, string[] args, Mode mode)
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
                     string.Format(
                         "REQUIRED: ConnectionString - As an alternative to ServerName and Database - You can provide an entire connection string instead."),
                     option => configuration.ConnectionString = option)
                .Add("f=|files=|sqlfilesdirectory=",
                     string.Format("SqlFilesDirectory - The directory where your SQL scripts are. Defaults to \"{0}\".",
                                   ApplicationParameters.default_files_directory),
                     option => configuration.SqlFilesDirectory = option)
                .Add("s=|server=|servername=|instance=|instancename=",
                     string.Format(
                         "ServerName - The server and instance you would like to run on. (local) and (local)\\SQL2008 are both valid values. Defaults to \"{0}\".",
                         ApplicationParameters.default_server_name),
                     option => configuration.ServerName = option)
                .Add("csa=|connstringadmin=|connectionstringadministration=",
                     string.Format(
                         "ConnectionStringAdministration - This is used for connecting to master when you may have a different uid and password than normal."),
                     option => configuration.ConnectionStringAdmin = option)
                .Add("accesstoken=",
                     "AccessToken - This connection property is used to connect to a SQL Database using an access token (for example Azure AD token).",
                     option => configuration.AccessToken = option)
                .Add("ct=|commandtimeout=",
                     string.Format(
                         "CommandTimeout - This is the timeout when commands are run. This is not for admin commands or restore. Defaults to \"{0}\".",
                         ApplicationParameters.default_command_timeout),
                     option => configuration.CommandTimeout = int.Parse(option))
                .Add("cta=|commandtimeoutadmin=",
                     string.Format(
                         "CommandTimeoutAdministration - This is the timeout when administration commands are run (except for restore, which has its own). Defaults to \"{0}\".",
                         ApplicationParameters.default_admin_command_timeout),
                     option => configuration.CommandTimeoutAdmin = int.Parse(option))
                //database type
                .Add("dt=|dbt=|databasetype=",
                     string.Format(
                         "DatabaseType - Tells RH what type of database it is running on. This is a plugin model. This is the fully qualified name of a class that implements the interface roundhouse.sql.Database, roundhouse. If you have your own assembly, just set it next to rh.exe and set this value appropriately. Defaults to 'sqlserver' which is a synonym for '{0}'.",
                         ApplicationParameters.default_database_type),
                     option => configuration.DatabaseType = option)
                // versioning
                .Add("r=|repo=|repositorypath=",
                     string.Format(
                         "RepositoryPath - The repository. A string that can be anything. Used to track versioning along with the version. Defaults to null."),
                     option => configuration.RepositoryPath = option)
                .Add("v=|version=",
                     "Version - Specify the version directly instead of looking in a file. If present, ignores file version options.",
                     option => configuration.Version = option)
                .Add("vf=|versionfile=",
                     string.Format("VersionFile - Either a .XML file, a .DLL or a .TXT or a .json file that a version can be resolved from. Defaults to \"{0}\".",
                                   ApplicationParameters.default_version_file),
                     option => configuration.VersionFile = option)
                .Add("vx=|versionxpath=",
                     string.Format("VersionXPath - Works in conjunction with an XML version file. Defaults to \"{0}\".",
                                   ApplicationParameters.default_version_x_path),
                     option => configuration.VersionXPath = option)
                // folders
                .Add("ad=|alterdatabase=|alterdatabasefolder=|alterdatabasefoldername=",
                     string.Format(
                         "AlterDatabaseFolderName - The name of the folder where you keep your alter database scripts. Read up on token replacement. You will want to use {{DatabaseName}} here instead of specifying a database name. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_alter_database_folder_name),
                     option => configuration.AlterDatabaseFolderName = option)
                .Add("racd=|runaftercreatedatabase=|runaftercreatedatabasefolder=|runaftercreatedatabasefoldername=",
                     string.Format(
                         "RunAfterCreateDatabaseFolderName - The name of the folder where you will keep scripts that ONLY run after a database is created.  Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_run_after_create_database_folder_name),
                     option => configuration.RunAfterCreateDatabaseFolderName = option)
                .Add("rb=|runbefore=|runbeforeupfolder=|runbeforeupfoldername=",
                     string.Format(
                         "RunBeforeUpFolderName - The name of the folder where you keep scripts that you want to run before your update scripts. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_run_before_up_folder_name),
                     option => configuration.RunBeforeUpFolderName = option)
                .Add("u=|up=|upfolder=|upfoldername=",
                     string.Format(
                         "UpFolderName - The name of the folder where you keep your update scripts. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_up_folder_name),
                     option => configuration.UpFolderName = option)
                .Add("do=|down=|downfolder=|downfoldername=",
                     string.Format(
                         "DownFolderName - The name of the folder where you keep your versioning down scripts. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_down_folder_name),
                     option => configuration.DownFolderName = option)
                .Add("rf=|runfirst=|runfirstfolder=|runfirstafterupdatefolder=|runfirstafterupdatefoldername=",
                     string.Format(
                         "RunFirstAfterUpdateFolderName - The name of the folder where you keep any functions, views, or sprocs that are order dependent. If you have a function that depends on a view, you definitely need the view in this folder. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_run_first_after_up_folder_name),
                     option => configuration.RunFirstAfterUpFolderName = option)
                .Add("fu=|functions=|functionsfolder=|functionsfoldername=",
                     string.Format(
                         "FunctionsFolderName - The name of the folder where you keep your functions. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_functions_folder_name),
                     option => configuration.FunctionsFolderName = option)
                .Add("vw=|views=|viewsfolder=|viewsfoldername=",
                     string.Format("ViewsFolderName - The name of the folder where you keep your views. Will recurse through subfolders. Defaults to \"{0}\".",
                                   ApplicationParameters.default_views_folder_name),
                     option => configuration.ViewsFolderName = option)
                .Add("sp=|sprocs=|sprocsfolder=|sprocsfoldername=",
                     string.Format(
                         "SprocsFolderName - The name of the folder where you keep your stored procedures. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_sprocs_folder_name),
                     option => configuration.SprocsFolderName = option)
                .Add("trg=|triggers=|triggersfolder=|triggersfoldername=",
                     string.Format(
                         "TriggersFolderName - The name of the folder where you keep your triggers. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_triggers_folder_name),
                     option => configuration.TriggersFolderName = option)
                .Add("ix=|indexes=|indexesfolder=|indexesfoldername=",
                     string.Format(
                         "IndexesFolderName - The name of the folder where you keep your indexes. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_indexes_folder_name),
                     option => configuration.IndexesFolderName = option)
                .Add("ra=|runAfterOtherAnyTimeScripts=|runAfterOtherAnyTimeScriptsfolder=|runAfterOtherAnyTimeScriptsfoldername=",
                     string.Format(
                         "RunAfterOtherAnyTimeScriptsFolderName - The name of the folder where you keep scripts that will be run after all of the other any time scripts complete. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_runAfterOtherAnyTime_folder_name),
                     option => configuration.RunAfterOtherAnyTimeScriptsFolderName = option)
                .Add("p=|permissions=|permissionsfolder=|permissionsfoldername=",
                     string.Format(
                         "PermissionsFolderName - The name of the folder where you keep your permissions scripts. Will recurse through subfolders. Defaults to \"{0}\".",
                         ApplicationParameters.default_permissions_folder_name),
                     option => configuration.PermissionsFolderName = option)
                .Add("bmg=|beforemig=|beforemigrationfolder=|beforemigrationfoldername=",
                         "BeforeMigrationFolderName - The name of the folder where you keep your scripts that needs to run before migration. Script will run outside of transaction. Will recurse through subfolders.",
                     option => configuration.BeforeMigrationFolderName = option)
                .Add("amg=|aftermig=|aftermigrationfolder=|aftermigrationfoldername=",
                         "AfterMigrationFolderName - The name of the folder where you keep your scripts that needs to run after migration. Script will run outside of transaction. Will recurse through subfolders.",
                     option => configuration.AfterMigrationFolderName = option)
                // roundhouse items
                .Add("sc=|schema=|schemaname=",
                     string.Format(
                         "SchemaName - This is the schema where RH stores it's tables. Once you set this a certain way, do not change this. This is definitely running with scissors and very sharp. I am allowing you to have flexibility, but because this is a knife you can still get cut if you use it wrong. I'm just saying. You've been warned. Defaults to \"{0}\".",
                         ApplicationParameters.default_roundhouse_schema_name),
                     option => configuration.SchemaName = option)
                .Add("vt=|versiontable=|versiontablename=",
                     string.Format(
                         "VersionTableName - This is the table where RH stores versioning information. Once you set this, do not change this. This is definitely running with scissors and very sharp. Defaults to \"{0}\".",
                         ApplicationParameters.default_version_table_name),
                     option => configuration.VersionTableName = option)
                .Add("srt=|scriptsruntable=|scriptsruntablename=",
                     string.Format(
                         "ScriptsRunTableName - This is the table where RH stores information about scripts that have been run. Once you set this a certain way, do not change this. This is definitely running with scissors and very sharp. Defaults to \"{0}\".",
                         ApplicationParameters.default_scripts_run_table_name),
                     option => configuration.ScriptsRunTableName = option)
                .Add("sret=|scriptsrunerrorstable=|scriptsrunerrorstablename=",
                     string.Format(
                         "ScriptsRunErrorsTableName - This is the table where RH stores information about scripts that have been run with errors. Once you set this a certain way, do not change this. This is definitelly running with scissors and very sharp. Defaults to \"{0}\".",
                         ApplicationParameters.default_scripts_run_errors_table_name),
                     option => configuration.ScriptsRunErrorsTableName = option)
                //environment(s)
                .Add("env=|environment=|environmentname=|envs=|environments=|environmentnames=",
                     string.Format(
                         "EnvironmentName(s) - This allows RH to be environment aware and only run scripts that are in a particular environment based on the naming of the script. LOCAL.something.ENV.sql would only be run in the LOCAL environment. Multiple environments may be specified as a comma or semicolon separated list. Defaults to \"{0}\".",
                         ApplicationParameters.default_environment_name),
                     environmentOption =>
                     {
                         foreach (var environment in environmentOption.Split(OptionsSplit, StringSplitOptions.RemoveEmptyEntries))
                         {
                             configuration.EnvironmentNames.Add(environment);
                         }
                     })
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
                //don't alter the database (e.g. to avoid needing master DB login)
                .Add("da|dna|donotalterdatabase",
                     "DoNotAlterDatabase - This instructs RH to not alter the database. Defaults to false.",
                     option => configuration.DoNotAlterDatabase = option != null)
                //output
                .Add("o=|output=|outputpath=",
                     string.Format(
                         "OutputPath - This is where everything related to the migration is stored. This includes any backups, all items that ran, permission dumps, logs, etc. Defaults to \"{0}\".",
                         ApplicationParameters.default_output_path),
                     option => configuration.OutputPath = option)
                //output
                .Add("disableoutput",
                     string.Format(
                         "DisableoOutput - Disable output of backups, items ran, permissions dumps, etc. Log files are kept. Useful for example in CI environment. Defaults to \"{0}\".",
                         ApplicationParameters.default_disable_output),
                     option => configuration.DisableOutput = option != null)
                //warn on changes
                .Add("w|warnononetimescriptchanges",
                     "WarnOnOneTimeScriptChanges - Instructs RH to execute changed one time scripts (DDL/DML in Up folder) that have previously been run against the database instead of failing. A warning is logged for each one time scripts that is rerun. Defaults to false.",
                     option => configuration.WarnOnOneTimeScriptChanges = option != null)
                //warn on changes
                .Add("warnandignoreononetimescriptchanges",
                     "WarnAndIgnoreOnOneTimeScriptChanges - Instructs RH to ignore and update the hash of changed one time scripts (DDL/DML in Up folder) that have previously been run against the database instead of failing. A warning is logged for each one time scripts that is rerun. Defaults to false.",
                     option => configuration.WarnAndIgnoreOnOneTimeScriptChanges = option != null)
                //silent?
                .Add("silent|ni|noninteractive",
                     "Silent - tells RH not to ask for any input when it runs. Defaults to false.",
                     option => configuration.Silent = option != null)
                //transaction
                .Add("t|trx|transaction|wt|withtransaction",
                     "WithTransaction - This instructs RH to run inside of a transaction. Defaults to false.",
                     option => configuration.WithTransaction = option != null)
                //recovery mode
                .Add("rcm=|recoverymode=",
                     "RecoveryMode - This instructs RH to set the database recovery mode to Simple|Full|NoChange. Defaults to NoChange.",
                     option => configuration.RecoveryMode = (RecoveryMode)Enum.Parse(typeof(RecoveryMode), option, true))
               //user tokens
               .Add("ut=|usertokens=",
                    "UserTokens - This is a list of key/value pairs used in scripts: the token '{{SomeToken}}' will be replace by 'value123' if 'ut=SomeToken=value123' is provided. Separate multiple tokens with ;",
                    option => configuration.UserTokens = UserTokenParser.Parse(option))
                //debug
                .Add("debug",
                     "Debug - This instructs RH to write out all messages. Defaults to false.",
                     option => configuration.Debug = option != null)
                //force all anytime scripts
                .Add("runallanytimescripts|forceanytimescripts",
                     "RunAllAnyTimeScripts - This instructs RH to run any time scripts every time it is run. Defaults to false.",
                     option => configuration.RunAllAnyTimeScripts = option != null)
                //disable token replacement
                .Add("disabletokens|disabletokenreplacement",
                     "DisableTokenReplacement - This instructs RH to not perform token replacement {{somename}}. Defaults to false.",
                     option => configuration.DisableTokenReplacement = option != null)
                //recorders
                .Add("baseline",
                     "Baseline - This instructs RH to create an insert for its recording tables, but not to actually run anything against the database. Use this option if you already have scripts that have been run through other means (and BEFORE you start the new ones).",
                     option => configuration.Baseline = option != null)
                .Add("dryrun",
                     "DryRun - This instructs RH to log what would have run, but not to actually run anything against the database. Use this option if you are trying to figure out what RH is going to do.",
                     option => configuration.DryRun = option != null)
                .Add("searchallinsteadoftraverse|searchallsubdirectoriesinsteadoftraverse",
                     "SearchAllSubdirectoriesInsteadOfTraverse - Each Migration folder's subdirectories are traversed by default. This option pulls back scripts from the main directory and all subdirectories at once. Defaults to 'false'",
                     option => configuration.SearchAllSubdirectoriesInsteadOfTraverse = option != null)
                .Add("isuptodate",
                     "This option prints whether there are any database updates or not, without actually running them. Other output except errors is disabled, to make it easy to use in scripts.",
                     option => { })
                //do not store full text of run scripts
                .Add("donotstorescriptsruntext",
                     "DoNotStoreScriptsRunText - This instructs RH to not store the full script text in the database. Defaults to false.",
                     option => configuration.DoNotStoreScriptsRunText = option != null)
                // default encoding
                .Add("defaultencoding=",
                     "Default encoding to use for loading script file from disk if file doesn't contain BOM. For the list of possible values see the column Name in table listed in .NET Encoding class documentation. Defaults to UTF-8",
                     option =>
                         {
                             if(option != null)
                             {
                                 configuration.DefaultEncoding = System.Text.Encoding.GetEncoding(option);
                             }
                         })
                 .Add("cse|connectionstringbase64encoded",
                     "Use Base64 encoded connection string to avoid issues with special symbols",
                    option =>
                    {
                        if (option != null)
                        {
                            configuration.ConnectionString = Encoding.UTF8.GetString(Convert.FromBase64String(configuration.ConnectionString));
                        }
                    })
                .Add("csae|connectionstringadminbase64encoded",
                     "Use Base64 encoded admin connection string to avoid issues with special symbols",
                    option =>
                    {
                        if (option != null)
                        {
                            configuration.ConnectionStringAdmin = Encoding.UTF8.GetString(Convert.FromBase64String(configuration.ConnectionStringAdmin));
                        }
                    })
                //load configuration from file
                .Add("cf=|configfile=|configurationfile=",
                    "Loads configuration options from a JSON file",
                    option => configuration.ConfigurationFile = option)
                ;

            try
            {
                option_set = merge_configuration_file(args, option_set, configuration);
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
                        "/accesstoken VALUE " +
                        "/c[ommand]t[imeout] VALUE /c[ommand]t[imeout]a[dmin] VALUE " +
                        "/r[epositorypath] VALUE /v[ersion] VALUE /v[ersion]f[ile] VALUE /v[ersion]x[path] VALUE " +
                        "/a[lter]d[atabasefoldername] /r[un]a[fter]c[reate]d[atabasefoldername] VALUE VALUE " +
                        "/r[un]b[eforeupfoldername] VALUE /u[pfoldername] VALUE /do[wnfoldername] VALUE " +
                        "/r[un]f[irstafterupdatefoldername] VALUE /fu[nctionsfoldername] VALUE /v[ie]w[sfoldername] VALUE " +
                        "/sp[rocsfoldername] VALUE /i[nde]x[foldername] VALUE /p[ermissionsfoldername] VALUE " +
                        "/sc[hemaname] VALUE /v[ersion]t[ablename] VALUE /s[cripts]r[un]t[ablename] VALUE /s[cripts]r[un]e[rrors]t[ablename] VALUE " +
                        "/env[ironmentname] VALUE " +
                        "/restore /r[estore]f[rom]p[ath] VALUE /r[estore]c[ustom]o[ptions] VALUE /r[estore]t[imeout] VALUE" +
                        "/c[reate]d[atabasecustom]s[cript] VALUE " +
                        "/env[ironmentname] VALUE " +
                        "/o[utputpath] VALUE " +
                        "/w[arnononetimescriptchanges] " +
                        "/silent " +
                        "/pde[PerDirectoryExecution] " +
                        "/d[atabase]t[ype] VALUE " +
                        "/drop " +
                        "/d[onot]c[reatedatabase] " +
                        "/t[ransaction] " +
                        "/r[e]c[overy]m[ode] NoChange|Simple|Full" +
                        "/debug " +
                        "/runallanytimescripts " +
                        "/disabletokenreplacement " +
                        "/baseline " +
                        "/dryrun " +
                        "/search[allsubdirectories]insteadoftraverse " + 
                        "/c[onnection]s[tringbase64]e[ncoded] " +
                        "/c[onnection]s[tring]a[dminbase64]e[ncoded] " +
                        "/isuptodate" +
                        "/defaultencoding VALUE" +
                        "]", Environment.NewLine);
                show_help(usage_message, option_set);
            }

            if (string.IsNullOrEmpty(configuration.DatabaseName) && string.IsNullOrEmpty(configuration.ConnectionString) && mode == Mode.Normal)
            {
                show_help("Error: You must specify Database Name (/d) OR Connection String (/cs) at a minimum to use RoundhousE.", option_set);
            }

            if (configuration.Restore && string.IsNullOrEmpty(configuration.RestoreFromPath))
            {
                show_help(
                    "If you set Restore to true, you must specify a location for the database to be restored from (RestoreFromPath /restorefrompath).",
                    option_set);
            }
        }

        private static OptionSet merge_configuration_file(string[] args, OptionSet option_set, ConfigurationPropertyHolder configuration)
        {
            option_set.Parse(args);
            if(!string.IsNullOrEmpty(configuration.ConfigurationFile))
            {
                bool any_update = false;
                List<string> new_args = new List<string>(args);
                try 
                {
                    if(!System.IO.File.Exists(configuration.ConfigurationFile))
                    {
                        throw new Exception("Configuration File does not exist: " + configuration.ConfigurationFile);
                    }
                    var json_Data = System.IO.File.ReadAllText(configuration.ConfigurationFile);
                    var file_values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json_Data);
                    foreach(var key in file_values.Keys)
                    {
                        if(!string.IsNullOrEmpty(file_values[key]))
                        {
                            var matched_option = find_option_by_prototype(option_set, key);
                            if(matched_option != null)
                            {
                                if(add_arg_if_not_exist(new_args, option_set, matched_option, file_values[key]))
                                {
                                    any_update = true;
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {
                    show_help(ex.Message, option_set);
                }
                if(any_update)
                {
                    option_set.Parse(new_args);
                }
            }
            return option_set;
		}

        private static bool add_arg_if_not_exist(List<string> args, OptionSet option_set, Option option_to_replace, string new_value)
        {
            for(int i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                if(!string.IsNullOrEmpty(arg))
                {
                    string arg_flag;
                    string arg_name;
                    string arg_sep;
                    string arg_value;
                    if(option_set.GetOptionParts(arg, out arg_flag, out arg_name, out arg_sep, out arg_value))
                    {
                        foreach(var name in option_to_replace.GetNames())
                        {
                            if(!string.IsNullOrEmpty(name) && name.Equals(arg_name, StringComparison.CurrentCultureIgnoreCase))
                            {
                                //Value already exists, skip this, command line should take precedence.
                                return false;
                            }
                        }
                    }
                }
            }
            string new_arg = "/" + option_to_replace.GetNames()[0]  + "=" + new_value;
            args.Add(new_arg);
            return true;
        }

        private static Option find_option_by_prototype(OptionSet option_set, string key)
        {
            foreach(var option in option_set)
            {
                foreach(var name in option.GetNames())
                {
                    if(!string.IsNullOrEmpty(name)  && name.StartsWith(key, StringComparison.CurrentCultureIgnoreCase))
                    {
                        return option;
                    }
                }
            }
            return null;
        }
		
		public static void show_help(string message, OptionSet option_set)
        {
            //Console.Error.WriteLine(message);
            the_logger.Info(message);
            option_set.WriteOptionDescriptions(Console.Error);
            Environment.Exit(-1);
        }

        public static void init_folder(ConfigurationPropertyHolder configuration)
        {
            the_logger.Info("Initializing folder for roundhouse");
            Container.get_an_instance_of<Initializer>().Initialize(configuration, ".");
            Environment.Exit(0);
        }
        
        private static void change_log_level(Level level)
        {
            ILoggerRepository log_repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
            log_repository.Threshold = level;
            foreach (ILogger log in log_repository.GetCurrentLoggers())
            {
                var logger = log as log4net.Repository.Hierarchy.Logger;
                if (logger != null)
                {
                    logger.Level = level;
                }
            }
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

        public static void init_security_protocol()
        {
            // allow tls
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        private static void run_update_check(ConfigurationPropertyHolder configuration)
        {
            if (!configuration.Silent)
            {
                Console.WriteLine("NOTE: Running this command will create the Roundhouse tables, if they don't exist.");
                Console.WriteLine("Please press enter when ready to kick...");
                Console.ReadLine();
            }

            // Info and warn level logging is turned off, in order to make it easy to use the output of this command.
            change_log_level(Level.Error);

            RoundhouseUpdateCheckRunner update_check_runner = get_update_check_runner(configuration, get_migration_runner(configuration));
            update_check_runner.run();

            if (!configuration.Silent)
            {
                Console.WriteLine("{0}Please press enter to continue...", Environment.NewLine);
                Console.Read();
            }
        }

        private static RoundhouseMigrationRunner get_migration_runner(ConfigurationPropertyHolder configuration)
        {
            return new RoundhouseMigrationRunner(
                configuration.RepositoryPath,
                Container.get_an_instance_of<environments.EnvironmentSet>(),
                Container.get_an_instance_of<KnownFolders>(),
                Container.get_an_instance_of<FileSystemAccess>(),
                Container.get_an_instance_of<DatabaseMigrator>(),
                Container.get_an_instance_of<VersionResolver>(),
                configuration.Silent,
                configuration.Drop,
                configuration.DoNotCreateDatabase,
                configuration.WithTransaction,
                configuration);
        }

        private static RoundhouseRedGateCompareRunner get_diff_runner(ConfigurationPropertyHolder configuration, RoundhouseMigrationRunner migration_runner)
        {
            return new RoundhouseRedGateCompareRunner(
                Container.get_an_instance_of<KnownFolders>(),
                Container.get_an_instance_of<FileSystemAccess>(),
                configuration, migration_runner);
        }

        private static RoundhouseUpdateCheckRunner get_update_check_runner(ConfigurationPropertyHolder configuration, RoundhouseMigrationRunner migration_runner)
        {
            return new RoundhouseUpdateCheckRunner(
                Container.get_an_instance_of<environments.EnvironmentSet>(),
                Container.get_an_instance_of<KnownFolders>(),
                Container.get_an_instance_of<FileSystemAccess>(),
                Container.get_an_instance_of<DatabaseMigrator>(), 
                configuration,
                migration_runner);
        }
    }
}
