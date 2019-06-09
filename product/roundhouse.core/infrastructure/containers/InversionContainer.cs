namespace roundhouse.infrastructure.containers
{
    public interface InversionContainer
    {
        TypeToReturn Resolve<TypeToReturn>();
    }
}