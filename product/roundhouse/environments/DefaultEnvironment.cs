using roundhouse.infrastructure.extensions;

namespace roundhouse.environments
{
    using infrastructure.app;

    public sealed class DefaultEnvironment : Environment
    {
        public DefaultEnvironment(ConfigurationPropertyHolder configuration)
        {
            name = configuration.EnvironmentName;
        }

        public string name { get; private set; }

        public bool item_is_for_this_environment(string item_name)
        {
            return name.to_lower() == item_name.Substring(0, item_name.IndexOf('.')).to_lower();
        }
    }
}