using System;

using roundhouse.folders;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;
using roundhouse.migrators;
using Environment = roundhouse.environments.Environment;

namespace roundhouse.runners
{
    public class RoundhouseUpdateCheckRunner : IRunner
    {
        private readonly ConfigurationPropertyHolder configuration;
        private readonly Environment environment;
        private readonly KnownFolders known_folders;
        private readonly FileSystemAccess file_system;
        private readonly DatabaseMigrator database_migrator;
        private readonly RoundhouseMigrationRunner migration_runner;

        private const string SQL_EXTENSION = "*.sql";


        public RoundhouseUpdateCheckRunner(
            Environment environment,
            KnownFolders known_folders, 
            FileSystemAccess file_system, 
            DatabaseMigrator database_migrator, 
            ConfigurationPropertyHolder configuration,
            RoundhouseMigrationRunner migration_runner)
        {
            this.environment = environment;
            this.known_folders = known_folders;
            this.file_system = file_system;
            this.database_migrator = database_migrator;
            this.configuration = configuration;
            this.migration_runner = migration_runner;
        }


        public void run()
        {
            bool is_up_to_date = is_database_up_to_date();

            // Info and warn level logging is turned off, in order to make it easy to use the output of this command.
            // This message can therefore not be printed using the info logging as used by most other output in roundhouse.
            Console.WriteLine("Database is up to date: {0}", is_up_to_date);
        }


        public bool is_database_up_to_date()
        {
            database_migrator.open_connection(false);

            // TODO: Consider if we should just return false if the roundhouse tables does not exist, instead of creating them.
            database_migrator.run_roundhouse_support_tasks();

            bool up_to_date = check_folder(this.known_folders.alter_database)
                                      && check_folder(this.known_folders.up)
                                      && check_folder(this.known_folders.run_first_after_up)
                                      && check_folder(this.known_folders.functions)
                                      && check_folder(this.known_folders.views)
                                      && check_folder(this.known_folders.sprocs)
                                      && check_folder(this.known_folders.indexes)
                                      && check_folder(this.known_folders.run_after_other_any_time_scripts)
                                      && check_folder(this.known_folders.permissions);

            database_migrator.close_connection();

            return up_to_date;
        }

        private bool check_folder(MigrationsFolder migration_folder)
        {
            if (!file_system.directory_exists(migration_folder.folder_full_path)) return true;

            var file_names = file_system.get_all_file_name_strings_recurevly_in(migration_folder.folder_full_path, SQL_EXTENSION);
            foreach (string sql_file in file_names)
            {
                string sql_file_text = migration_runner.replace_tokens(migration_runner.get_file_text(sql_file));

                bool script_should_run = database_migrator.this_script_is_new_or_updated(
                    file_system.get_file_name_from(sql_file),
                    sql_file_text,
                    environment);

                if (script_should_run)
                    return false;
            }

            return true;
        }
    }
}