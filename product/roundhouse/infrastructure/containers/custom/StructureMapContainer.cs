namespace roundhouse.infrastructure.containers.custom
{
    using StructureMap;

    public sealed class StructureMapContainer : InversionContainer
    {
        private readonly IContainer the_container;

        public StructureMapContainer(IContainer the_container)
        {
            this.the_container = the_container;
        }

        public TypeToReturn Resolve<TypeToReturn>()
        {
            return the_container.GetInstance<TypeToReturn>();
        }
    }
}