namespace roundhouse.infrastructure.containers
{
    public static class Container
    {
        private static InversionContainer the_container;

        public static void initialize_with(InversionContainer container)
        {
            the_container = container;
        }

        public static TypeToGet get_an_instance_of<TypeToGet>()
        {
            return the_container.Resolve<TypeToGet>();
        }
    }
}