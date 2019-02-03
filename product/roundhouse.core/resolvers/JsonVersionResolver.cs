using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;

namespace roundhouse.resolvers
{
    public class JsonVersionResolver : VersionResolver
    {
        private readonly FileSystemAccess file_system;
        private readonly string version_file;

        public JsonVersionResolver(FileSystemAccess file_system, string version_file)
        {
            this.file_system = file_system;
            this.version_file = file_system.get_full_path(version_file);
        }

        public bool meets_criteria()
        {
            return version_file.EndsWith(".json", StringComparison.OrdinalIgnoreCase);
        }

        public string resolve_version()
        {
            Log.bound_to(this).log_an_info_event_containing(
                " Attempting to resolve version from json file {0}.", version_file);

            string version = "0";
            if (file_system.file_exists(version_file))
            {
                try
                {
                    string json = file_system.read_file_text(version_file);
                    JsonVersionNumber item = JsonConvert.DeserializeObject<JsonVersionNumber>(json);
                    version = item.version;
                    Log.bound_to(this).log_an_info_event_containing(" Found version {0} from {1}.", version, version_file);
                }
                catch (Exception e)
                {
                    Log.bound_to(this).log_an_error_event_containing(
                        "Unable to get version from json file {0}:{1}{2}", version_file, Environment.NewLine, e.Message);
                }
            }
            else
            {
                Log.bound_to(this).log_a_warning_event_containing(
                    "Unable to get version from json file {0}. File doesn't exist.", version_file);
            }

            return version;
        }
    }
    public class JsonVersionNumber
    {
        public string version { get; set; }
    }

}