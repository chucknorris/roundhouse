namespace roundhouse.infrastructure.containers
{
    using System;

    public static class Container
    {
        private static InversionContainer the_container;

        public static void initialize_with(InversionContainer container)
        {
            the_container = container;
        }

        public static TypeToGet get_an_instance_of<TypeToGet>()
        {
            if (the_container == null) throw new NullReferenceException("The container has not been initialized yet, can't return anything.");
            return the_container.Resolve<TypeToGet>();
        }
    }
}