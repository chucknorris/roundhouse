using System;

using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;

namespace roundhouse.resolvers
{
    public class TextVersionResolver : VersionResolver
    {
        private readonly FileSystemAccess file_system;
        private readonly string version_file;

        public TextVersionResolver(FileSystemAccess file_system, string version_file)
        {
            this.file_system = file_system;
            this.version_file = version_file;
        }

        public bool meets_criteria()
        {
            return version_file.EndsWith(".txt", StringComparison.OrdinalIgnoreCase);
        }

        public string resolve_version()
        {
            Log.bound_to(this).log_an_info_event_containing(
                " Attempting to resolve version from text file {0}.", version_file);

            string version = "0";
            if (file_system.file_exists(version_file))
            {
                try
                {
                    version = file_system.read_file_text(version_file);
                    Log.bound_to(this).log_an_info_event_containing(" Found version {0} from {1}.", version, version_file);
                }
                catch (Exception e)
                {
                    Log.bound_to(this).log_an_error_event_containing(
                        "Unable to get version from text file {0}:{1}{2}", version_file, Environment.NewLine, e.Message);
                }
            }
            else
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "Unable to get version from text file {0}. File doesn't exist.", version_file);
            }

            return version;
        }
    }
}