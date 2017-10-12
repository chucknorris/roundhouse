namespace roundhouse.environments
{
    using System.Collections.Generic;
    using System.Linq;
    using infrastructure.app;

    public sealed class DefaultEnvironmentSet : EnvironmentSet
    {
        public IEnumerable<Environment> set_items { get; private set; }

        public DefaultEnvironmentSet(ConfigurationPropertyHolder configuration_property_holder)
        {
            set_items = configuration_property_holder.EnvironmentNames
                .Split(',')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => new DefaultEnvironment(x.Trim()))
                .ToList();
        }

        public bool item_is_for_this_environment_set(string item_name)
        {
            return set_items.Any(set_item => set_item.item_is_for_this_environment(item_name));
        }
    }
}