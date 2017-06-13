using roundhouse.infrastructure.logging;

namespace roundhouse.resolvers
{
    public sealed class CommandLineVersionResolver : VersionResolver
    {
        private readonly string _version;

        public CommandLineVersionResolver(string version)
        {
            _version = version;
        }
        public bool meets_criteria()
        {
            return !string.IsNullOrEmpty(_version);
        }

        public string resolve_version()
        {
            Log.bound_to(this).log_an_info_event_containing(
                " Found version {0} from command line argument.", _version);

            return _version;
        }
    }
}
