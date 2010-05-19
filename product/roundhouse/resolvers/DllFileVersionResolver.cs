using roundhouse.infrastructure.extensions;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;

namespace roundhouse.resolvers
{
    public sealed class DllFileVersionResolver : VersionResolver
    {
        private readonly FileSystemAccess file_system;
        private readonly string version_file;
        private const string dll_extension = ".dll";

        public DllFileVersionResolver(FileSystemAccess file_system, string version_file)
        {
            this.file_system = file_system;
            this.version_file = file_system.get_full_path(version_file);
        }

        public bool meets_criteria()
        {
            if (version_file_is_dll(version_file))
            {
                return true;
            }

            return false;
        }

        public string resolve_version()
        {
            string version = "0";

            Log.bound_to(this).log_an_info_event_containing(
                " Attempting to resolve assembly file version from {0}.", version_file);

            version = file_system.get_file_version_from(version_file);
            Log.bound_to(this).log_an_info_event_containing(" Found version {0} from {1}.", version, version_file);

            return version;
        }

        private bool version_file_is_dll(string version_file)
        {
            return file_system.get_file_extension_from(version_file).to_lower() == dll_extension;
        }
    }
}