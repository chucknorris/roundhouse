using System;
using System.IO;
using System.Linq;
using roundhouse.infrastructure.app;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;

namespace roundhouse.resolvers
{
    public class ScriptfileVersionResolver : VersionResolver
    {
        private readonly string version_file;
        private readonly FileSystemAccess file_system;
        private readonly string up_folder;
        private const string EXTENSION = "sql";

        public ScriptfileVersionResolver(FileSystemAccess file_system,
            ConfigurationPropertyHolder configuration_property_holder)
        {
            this.file_system = file_system;
            version_file = configuration_property_holder.VersionFile;
            up_folder = file_system.combine_paths(configuration_property_holder.SqlFilesDirectory,
                configuration_property_holder.UpFolderName);
        }

        public bool meets_criteria()
        {
            return version_file.Equals(EXTENSION, StringComparison.InvariantCultureIgnoreCase);
        }

        public string resolve_version()
        {
            var version = "0";
            var ver = new Version();

            if (file_system.directory_exists(up_folder))
            {
                var files = file_system.get_directory_info_from(up_folder)
                    .GetFiles("*." + EXTENSION).OrderBy(x => x.Name);
                long max = 0;
                foreach (var file in files)
                {
                    string file_name = Path.GetFileNameWithoutExtension(file.Name);
                    if (file_name.Contains("_")) file_name = file_name.Substring(0, file_name.IndexOf("_"));
                    if (file_name.Contains(".")) //using ##.##.##_description.sql format
                    {
                        var tmp = new Version(file_name);
                        if (tmp > ver)
                        {
                            ver = tmp;
                            version = ver.ToString();
                        }
                    }
                    else //using ###_description.sql format
                    {
                        long fileNumber = 0;
                        long.TryParse(file_name, out fileNumber);
                        max = Math.Max(max, fileNumber);
                        version = max.ToString();
                    }
                }
                Log.bound_to(this).log_an_info_event_containing(" Found version {0} from up directory.", version);
            }
            return version;
        }
    }
}