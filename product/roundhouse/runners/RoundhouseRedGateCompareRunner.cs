namespace roundhouse.runners
{
    using System.Collections.Generic;
    using System.Linq;
    using consoles;
    using folders;
    using infrastructure.app;
    using infrastructure.extensions;
    using infrastructure.filesystem;
    using Environment = environments.Environment;

    public class RoundhouseRedGateCompareRunner : IRunner
    {
        private readonly KnownFolders known_folders;
        private readonly FileSystemAccess file_system;
        private readonly ConfigurationPropertyHolder configuration;
        private readonly RoundhouseMigrationRunner migration_runner;
        private readonly string redgate_install_location = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ProgramFiles) + @"\Red Gate\SQL Compare 8";
        private const string redgate_compare_tool = @"sqlcompare.exe";

        public RoundhouseRedGateCompareRunner(KnownFolders known_folders, FileSystemAccess file_system, ConfigurationPropertyHolder configuration, RoundhouseMigrationRunner migration_runner)
        {
            this.known_folders = known_folders;
            this.file_system = file_system;
            this.configuration = configuration;
            this.migration_runner = migration_runner;
        }


        public void run()
        {
            string database_name = migration_runner.database_migrator.database.database_name;

            // run roundhouse to drop a database + base
            migration_runner.database_migrator.database.database_name += "_BASE";
            migration_runner.silent = true;
            migration_runner.dropping_the_database = true;
            migration_runner.run();
            // run roundhouse to create a database + base
            migration_runner.silent = true;
            migration_runner.dropping_the_database = false;
            migration_runner.run();
            // get the next number from the folder
            string next_change_number = get_new_change_number();
            // set up your arguments

            string redgate_args = string.Format(@"/database1:""{0}"" /database2:""{0}_BASE"" /include:table /exclude:table:\[{1}\]^|\[{2}\]^|\[{3}\] /exclude:view /options:Default,IgnoreConstraintNames,IgnorePermissions /ignoreparsererrors /f /scriptfile:""{4}\{5}_Changes.sql""", database_name, configuration.VersionTableName, configuration.ScriptsRunTableName, configuration.ScriptsRunErrorsTableName, file_system.get_full_path(known_folders.up.folder_full_path), next_change_number);
            // run redgate
            CommandExecutor.execute(file_system.combine_paths(redgate_install_location, redgate_compare_tool), redgate_args, true);
            // run rouhdhouse to drop a database + base
            migration_runner.silent = true;
            migration_runner.dropping_the_database = true;
            migration_runner.run();
        }

        public string get_new_change_number()
        {
            string[] files = file_system.get_all_file_name_strings_in(known_folders.up.folder_full_path);
            IList<string> file_numbers = files.Select(file => file_system.get_file_name_from(file).Substring(0, file_system.get_file_name_from(file).IndexOf('_'))).ToList();

            int length_of_number_format = 0;
            decimal highest_number = 0;
            foreach (string file_number in file_numbers)
            {
                decimal test_number = 0;
                decimal.TryParse(file_number, out test_number);
                if (test_number != 0)
                {
                    if (test_number > highest_number)
                    {
                        highest_number = test_number;
                    }
                    length_of_number_format = file_number.Length;
                }
            }
            string next_change_number = (highest_number + 1).to_string().PadLeft(length_of_number_format, '0');

            return next_change_number;
        }
    }
}