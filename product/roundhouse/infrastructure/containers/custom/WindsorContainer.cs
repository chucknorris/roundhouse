namespace roundhouse.infrastructure.containers.custom
{
    using Castle.Windsor;

    public sealed class WindsorContainer : InversionContainer
    {
        private readonly IWindsorContainer the_container;

        public WindsorContainer(IWindsorContainer the_container)
        {
            this.the_container = the_container;
        }

        public TypeToReturn Resolve<TypeToReturn>()
        {
            return the_container.Resolve<TypeToReturn>();
        }
    }
}