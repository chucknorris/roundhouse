using Ninject;

namespace roundhouse.infrastructure.containers.custom
{

    public sealed class NinjectContainer : InversionContainer
    {
        private readonly IKernel kernel;

        public NinjectContainer(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public TypeToReturn Resolve<TypeToReturn>()
        {
            return kernel.Get<TypeToReturn>();
        }
    }
}