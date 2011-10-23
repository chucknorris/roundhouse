using System;
using System.Text.RegularExpressions;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;

namespace roundhouse.resolvers
{
    public class ScriptfileVersionResolver : VersionResolver
    {
        private string _versionFile;
        private FileSystemAccess file_system;
        private string upFolder;

        public ScriptfileVersionResolver (FileSystemAccess file_system, string  versionFile, string upFolder)
        {
            _versionFile = versionFile;
            this.file_system = file_system;
            this.upFolder = upFolder;
        }

        public bool meets_criteria()
        {
            return _versionFile.Contains("up/*.sql");
        }

        public string resolve_version()
        {
            string version = "0";
            string extension = "sql";
            if(file_system.directory_exists(upFolder))
            {
                var files = file_system.get_directory_info_from(upFolder).GetFiles("*." + extension);
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