using System;
using System.Text.RegularExpressions;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;

namespace roundhouse.resolvers
{
    public class ScriptfileVersionResolver : VersionResolver
    {
        private string version_file;
        private FileSystemAccess file_system;
        private string up_folder;

        public ScriptfileVersionResolver (FileSystemAccess file_system, string  version_file, string upFolder)
        {
            this.version_file = version_file;
            this.file_system = file_system;
            this.up_folder = upFolder;
        }

        public bool meets_criteria()
        {
            return version_file.Contains("up/*.sql");
        }

        public string resolve_version()
        {
            string version = "0";
            string extension = "sql";
            if(file_system.directory_exists(up_folder))
            {
                var files = file_system.get_directory_info_from(up_folder).GetFiles("*." + extension);
                long max = 0;
                foreach(var file in files)
                {
                    var match = Regex.Match(file.Name, @"(\d+)_.*\." + extension, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    if(match.Success)
                    {
                        long fileNumber = Convert.ToInt64(match.Groups[1].Value);
                        max = Math.Max(max, fileNumber);
                    }
                }
                version = max.ToString();
                Log.bound_to(this).log_an_info_event_containing(" Found version {0} from up directory.", version);
            }
            return version;
        }
    }
}