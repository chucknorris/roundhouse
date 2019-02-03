namespace roundhouse.environments
{
    public interface Environment
    {
        string name {get;}
        bool item_is_for_this_environment(string item_name);
    }
}