using roundhouse.infrastructure.extensions;

namespace roundhouse.environments
{
    public sealed class DefaultEnvironment : Environment
    {
        public DefaultEnvironment(string environment_name)
        {
            name = environment_name;
        }

        public string name { get; private set; }

        public bool item_is_for_this_environment(string item_name)
        {
            if (item_name.to_lower().StartsWith(name.to_lower() + "."))
            {
                return true;
            }

            if (item_name.to_lower().Contains("." + name.to_lower() + "."))
            {
                return true;
            }

            return false;
        }
    }
}