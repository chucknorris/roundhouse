using System;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;
using System.Linq;

namespace roundhouse.resolvers
{
    using infrastructure.app;
    using System.IO;

    public class ScriptfileVersionResolver : VersionResolver
    {
        private string version_file;
        private FileSystemAccess file_system;
        private string up_folder;
        private string extension = "sql";

        public ScriptfileVersionResolver(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
        {
            this.file_system = file_system;
            this.version_file = configuration_property_holder.VersionFile;
            this.up_folder = file_system.combine_paths(configuration_property_holder.SqlFilesDirectory, configuration_property_holder.UpFolderName);
        }

        public bool meets_criteria()
        {
            return version_file.Equals(extension, StringComparison.InvariantCultureIgnoreCase);
        }

        public string resolve_version()
        {
            string version = "0";
            Version ver = new Version();
            
            if (file_system.directory_exists(up_folder))
            {
                var files = file_system.get_directory_info_from(up_folder)
                    .GetFiles("*." + extension).OrderBy(x => x.Name);
                long max = 0;
                foreach (var file in files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(file.Name);
                    if (fileName.Contains("_")) fileName = fileName.Substring(0, fileName.IndexOf("_"));
                    if (fileName.Contains(".")) //using ##.##.##_description.sql format
                    {
                        Version tmp = new Version(fileName);
                        if (tmp > ver)
                        {
                            ver = tmp;
                            version = ver.ToString();
                        }
                    }
                    else //using ###_description.sql format
                    {
                        long fileNumber = 0;
                        long.TryParse(fileName, out fileNumber);
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