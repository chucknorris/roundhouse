using roundhouse.infrastructure.extensions;

namespace roundhouse.environments
{
    public sealed class DefaultEnvironment : Environment
    {
        public DefaultEnvironment(string name)
        {
            this.name = name;
        }

        public string name { get; private set; }

        public bool item_is_for_this_environment(string item_name)
        {
            return name.to_lower() == item_name.Substring(0, item_name.IndexOf('.')).to_lower();
        }
    }
}