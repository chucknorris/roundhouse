namespace roundhouse.resolvers
{
    public interface VersionResolver
    {
        bool meets_criteria();
        string resolve_version();
    }
}