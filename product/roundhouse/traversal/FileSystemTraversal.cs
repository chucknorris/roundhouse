using System;
using System.Collections.Generic;
using System.IO;
using roundhouse.folders;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;

namespace roundhouse.traversal
{
    public class FileSystemTraversal : ITraversal
    {
        private const string SQL_EXTENSION = "*.sql";

        private readonly List<MigrationsFolder> all_folders;
        private readonly FileSystemAccess file_system;

        public FileSystemTraversal(KnownFolders folders, FileSystemAccess file_system)
        {
            this.file_system = file_system;
            all_folders = new List<MigrationsFolder>
                              {
                                  folders.up,
                                  folders.run_first_after_up,
                                  folders.functions,
                                  folders.views,
                                  folders.sprocs,
                                  folders.runAfterOtherAnyTimeScripts,
                                  folders.permissions
                              };
        }

        public void traverse(Action<IScriptInfo> script_action, Func<MigrationsFolder, bool> opt)
        {
            Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));
            Log.bound_to(this).log_an_info_event_containing("Migration Scripts");
            Log.bound_to(this).log_an_info_event_containing("{0}", "=".PadRight(50, '='));

            foreach (MigrationsFolder folder in all_folders)
            {
                if (!opt(folder))
                    continue;
                
                Log.bound_to(this).log_an_info_event_containing(
                    "Looking for {0} scripts in \"{1}\". {2}",
                    folder.friendly_name, folder.folder_full_path,
                    folder.should_run_items_in_folder_once ? "These should be one time only scripts." : string.Empty);
                Log.bound_to(this).log_an_info_event_containing("{0}", "-".PadRight(50, '-'));

                traverse_folder(folder, folder.folder_full_path, script_action);
            }
        }

        private void traverse_folder(MigrationsFolder folder, string directory, Action<IScriptInfo> script_action)
        {
            if (!file_system.directory_exists(directory)) 
                return;

            foreach (string sql_file in file_system.get_all_file_name_strings_in(directory, SQL_EXTENSION))
            {
                Log.bound_to(this).log_a_debug_event_containing(" Found {0}.", sql_file);
                script_action(
                    new ScriptFileInfo()
                        {
                            folder = folder,
                            script_name = file_system.get_file_name_from(sql_file),
                            script_contents = File.ReadAllText(sql_file)
                        }
                    );
            }

            foreach (string child_directory in file_system.get_all_directory_name_strings_in(directory))
                traverse_folder(folder, child_directory, script_action);
        }
    }
}