namespace roundhouse.environments
{
    using System.Collections.Generic;

    public interface EnvironmentSet
    {
        IEnumerable<Environment> set_items { get; }
        bool item_is_for_this_environment_set(string item_name);
    }
}