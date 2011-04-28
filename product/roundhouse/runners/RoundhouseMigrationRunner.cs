using roundhouse.folders;

namespace roundhouse.runners
{
    using System;
    using System.IO;
    using infrastructure;
    using infrastructure.filesystem;
    using infrastructure.logging;
    using migrators;
    using resolvers;
    using Environment = environments.Environment;

    public sealed class RoundhouseMigrationRunner : IRunner
    {
        private readonly string repository_path;
        private readonly Environment environment;
        private readonly KnownFolders known_folders;
        private readonly FileSystemAccess file_system;
        public DatabaseMigrator database_migrator { get; private set; }
        private readonly VersionResolver version_resolver;
        public bool silent { get; set; }
        public bool dropping_the_database { get; set; }
        private readonly bool dont_create_the_database;
        private bool run_in_a_transaction;
        private readonly bool use_simple_recovery;
        private const string SQL_EXTENSION = "*.sql";

        public RoundhouseMigrationRunner(
                string repository_path,
                Environment environment,
                KnownFolders known_folders,
                FileSystemAccess file_system,
                DatabaseMigrator database_migrator,
                VersionResolver version_resolver,
                bool silent,
                bool dropping_the_database,
                bool dont_create_the_database,
                bool run_in_a_transaction,
                bool use_simple_recovery)
        {
            this.known_folders = known_folders;
            this.repository_path = repository_path;
            this.environment = environment;
            this.file_system = file_system;
            this.database_migrator = database_migrator;
            this.version_resolver = version_resolver;
            this.silent = silent;
            this.dropping_the_database = dropping_the_database;
            this.dont_create_the_database = dont_create_the_database;
            this.run_in_a_transaction = run_in_a_transaction;
            this.use_simple_recovery = use_simple_recovery;
        }

        public void run()
        {
            database_migrator.initialize_connections();

            Log.bound_to(this).log_an_info_event_containing("Running {0} v{1} against {2} - {3}.",
                    ApplicationParameters.name,
                    infrastructure.VersionInformation.get_current_assembly_version(),
                    database_migrator.database.server_name,
                    database_migrator.database.database_name);

            Log.bound_to(this).log_an_info_event_containing("Looking in {0} for scripts to run.", known_folders.up.folder_path);
            if (!silent)
            {
                Log.bound_to(this).log_an_info_event_containing("Please press enter when ready to kick...");
                Console.ReadLine();
            }

            if (run_in_a_transaction && !database_migrator.database.supports_ddl_transactions)
            {
                Log.bound_to(this).log_a_warning_event_containing("You asked to run in a transaction, but this dabasetype doesn't support DDL transactions.");
                if (!silent)
                {
                    Log.bound_to(this).log_an_info_event_containing("Please press enter to continue without transaction support...");
                    Console.ReadLine();
                }
                run_in_a_transaction = false;
            }

            create_change_drop_folder();
            Log.bound_to(this).log_a_debug_event_containing("The change_drop (output) folder is: {0}", known_folders.change_drop.folder_full_path);

            try
            {
                Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
                Log.bound_to(this).log_an_info_event_containing("Setup, Backup, Create/Restore/Drop");
                Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
                create_share_and_set_permissions_for_change_drop_folder();
                //database_migrator.backup_database_if_it_exists();
                remove_share_from_change_drop_folder();

                if (!dropping_the_database)
                {
                    if (!dont_create_the_database)
                    {
                        database_migrator.open_admin_connection();
                        database_migrator.create_or_restore_database();
                        database_migrator.set_recovery_mode(use_simple_recovery);
                        database_migrator.close_admin_connection();
                    }
                    database_migrator.open_connection(run_in_a_transaction);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
                    Log.bound_to(this).log_an_info_event_containing("RoundhousE Structure");
                    Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
                    database_migrator.run_roundhouse_support_tasks();

                    Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
                    Log.bound_to(this).log_an_info_event_containing("Versioning");
                    Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
                    string current_version = database_migrator.get_current_version(repository_path);
                    string new_version = version_resolver.resolve_version();
                    Log.bound_to(this).log_an_info_event_containing(" Migrating {0} from version {1} to {2}.", database_migrator.database.database_name, current_version, new_version);
                    long version_id = database_migrator.version_the_database(repository_path, new_version);

                    Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
                    Log.bound_to(this).log_an_info_event_containing("Migration Scripts");
                    Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
                    Log.bound_to(this).log_an_info_event_containing("Looking for {0} scripts in \"{1}\". These should be one time only scripts.", "Update", known_folders.up.folder_full_path);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    traverse_files_and_run_sql(known_folders.up.folder_full_path, version_id, known_folders.up, environment, new_version);

                    //todo: remember when looking through all files below here, change CREATE to ALTER
                    // we are going to create the create if not exists script

                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    Log.bound_to(this).log_an_info_event_containing("Looking for {0} scripts in \"{1}\".", "Run First After Update", known_folders.run_first_after_up.folder_full_path);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    traverse_files_and_run_sql(known_folders.run_first_after_up.folder_full_path, version_id, known_folders.run_first_after_up, environment, new_version);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    Log.bound_to(this).log_an_info_event_containing("Looking for {0} scripts in \"{1}\".", "Function", known_folders.functions.folder_full_path);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    traverse_files_and_run_sql(known_folders.functions.folder_full_path, version_id, known_folders.functions, environment, new_version);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    Log.bound_to(this).log_an_info_event_containing("Looking for {0} scripts in \"{1}\".", "View", known_folders.views.folder_full_path);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    traverse_files_and_run_sql(known_folders.views.folder_full_path, version_id, known_folders.views, environment, new_version);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    Log.bound_to(this).log_an_info_event_containing("Looking for {0} scripts in \"{1}\".", "Stored Procedure", known_folders.sprocs.folder_full_path);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    traverse_files_and_run_sql(known_folders.sprocs.folder_full_path, version_id, known_folders.sprocs, environment, new_version);

                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    Log.bound_to(this).log_an_info_event_containing("Looking for {0} scripts in \"{1}\".", "Run after Other Anytime Scripts", known_folders.runAfterOtherAnyTimeScripts.folder_full_path);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    traverse_files_and_run_sql(known_folders.runAfterOtherAnyTimeScripts.folder_full_path, version_id, known_folders.runAfterOtherAnyTimeScripts, environment, new_version);
                    
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    Log.bound_to(this).log_an_info_event_containing("Looking for {0} scripts in \"{1}\". These scripts will run every time.", "Permission", known_folders.permissions.folder_full_path);
                    Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));
                    if (run_in_a_transaction)
                    {
                        database_migrator.close_connection();
                        database_migrator.open_connection(false);
                    }

                    traverse_files_and_run_sql(known_folders.permissions.folder_full_path, version_id, known_folders.permissions, environment, new_version);

                    Log.bound_to(this).log_an_info_event_containing("{0}{0}{1} v{2} has kicked your database ({3})! You are now at version {4}. All changes and backups can be found at \"{5}\".",
                                                System.Environment.NewLine,
                                                ApplicationParameters.name,
                                                infrastructure.VersionInformation.get_current_assembly_version(),
                                                database_migrator.database.database_name,
                                                new_version,
                                                known_folders.change_drop.folder_full_path);
                    database_migrator.close_connection();
                }
                else
                {
                    database_migrator.open_admin_connection();
                    database_migrator.delete_database();
                    database_migrator.close_admin_connection();
                    Log.bound_to(this).log_an_info_event_containing("{0}{0}{1} has removed database ({2}). All changes and backups can be found at \"{3}\".",
                                                System.Environment.NewLine,
                                                ApplicationParameters.name,
                                                database_migrator.database.database_name,
                                                known_folders.change_drop.folder_full_path);
                }
            }
            catch (Exception ex)
            {
                Log.bound_to(this).log_an_error_event_containing("{0} encountered an error.{1}{2}{3}",
                        ApplicationParameters.name,
                        run_in_a_transaction ? " You were running in a transaction though, so the database should be in the state it was in prior to this piece running. This does not include a drop/create or any creation of a database, as those items can not run in a transaction." : string.Empty,
                        System.Environment.NewLine,
                        ex.ToString());
                throw;
            }
            finally
            {
                copy_log_file_to_change_drop_folder();
            }
        }

        private void create_change_drop_folder()
        {
            file_system.create_directory(known_folders.change_drop.folder_full_path);
        }

        private void create_share_and_set_permissions_for_change_drop_folder()
        {
            //todo: implement creating share with change permissions
            //todo: implement setting Everyone to full acess to this folder
        }

        private void remove_share_from_change_drop_folder()
        {
            //todo: implement removal of the file share
        }

        //todo: understand what environment you are deploying to so you can decide what to run - it was suggested there be a specific tag in the file name. Like vw_something.ENV.sql and that be a static "ENV". Then to key off of the actual environment name on the front of the file (ex. TEST.vw_something.ENV.sql)
        //todo:down story

        public void traverse_files_and_run_sql(string directory, long version_id, MigrationsFolder migration_folder, Environment migrating_environment, string repository_version)
        {
            if (!file_system.directory_exists(directory)) return;

            foreach (string sql_file in file_system.get_all_file_name_strings_in(directory, SQL_EXTENSION))
            {
                string sql_file_text = File.ReadAllText(sql_file);
                Log.bound_to(this).log_a_debug_event_containing(" Found and running {0}.", sql_file);
                bool the_sql_ran = database_migrator.run_sql(sql_file_text, file_system.get_file_name_from(sql_file),
                                                             migration_folder.should_run_items_in_folder_once,
                                                             migration_folder.should_run_items_in_folder_every_time,
                                                             version_id, migrating_environment, repository_version, repository_path);
                if (the_sql_ran)
                {
                    try
                    {
                        copy_to_change_drop_folder(sql_file, migration_folder);
                    }
                    catch (Exception ex)
                    {
                        Log.bound_to(this).log_a_warning_event_containing("Unable to copy {0} to {1}. {2}{3}", sql_file, migration_folder.folder_full_path, System.Environment.NewLine, ex.ToString());
                    }
                }
            }

            foreach (string child_directory in file_system.get_all_directory_name_strings_in(directory))
            {
                traverse_files_and_run_sql(child_directory, version_id, migration_folder, migrating_environment, repository_version);
            }
        }

        private void copy_to_change_drop_folder(string sql_file_ran, Folder migration_folder)
        {
            string destination_file = file_system.combine_paths(known_folders.change_drop.folder_full_path, "itemsRan", sql_file_ran.Replace(migration_folder.folder_path + "\\", string.Empty));
            file_system.verify_or_create_directory(file_system.get_directory_name_from(destination_file));
            Log.bound_to(this).log_a_debug_event_containing("Copying file {0} to {1}.", file_system.get_file_name_from(sql_file_ran), destination_file);
            file_system.file_copy_unsafe(sql_file_ran, destination_file, true);
        }

        private void copy_log_file_to_change_drop_folder()
        {
            string log_file = ApplicationParameters.logging_file;
            string log_file_name = file_system.get_file_name_from(log_file);

            try
            {
                string destination_file = file_system.combine_paths(known_folders.change_drop.folder_full_path, log_file_name);
                file_system.file_copy(log_file, destination_file, true);
            }
            catch (Exception exception)
            {
                Log.bound_to(this).
                    log_an_error_event_containing("{0} encountered an error:{1}{2}",
                                                  ApplicationParameters.name,
                                                  System.Environment.NewLine,
                                                  exception.ToString()
                                                  );
            }

        }

    }
}