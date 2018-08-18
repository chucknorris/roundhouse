using Moq;

namespace roundhouse.tests.infrastructure.containers.custom
{
    using System;
    using roundhouse.infrastructure.containers.custom;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;
    using Ninject;
    using Container = roundhouse.infrastructure.containers.Container;

    public class NinjectContainerSpecs
    {
        public abstract class concern_for_structuremap_container : TinySpec<NinjectContainer>
        {
            [CLSCompliant(false)]
            protected IKernel the_kernel;
            protected Mock<IKernel> kernel_mock = new Mock<IKernel>();


            public override void AfterEachSpec()
            {
                Container.initialize_with(null);
            }
        }

        public abstract class concerns_using_a_fake_container : concern_for_structuremap_container
        {
            protected override NinjectContainer sut { get; set; }

            protected concerns_using_a_fake_container()
            {
                the_kernel = kernel_mock.Object;
                sut = new NinjectContainer(the_kernel);
            }
        }

        public abstract class concerns_using_a_real_container : concern_for_structuremap_container
        {
            protected override NinjectContainer sut { get; set; }

            public concerns_using_a_real_container()
            {
                the_kernel = new StandardKernel();
                sut = new NinjectContainer(the_kernel);
            }
        }

        [Concern(typeof(NinjectContainer))]
        public class when_the_kernel_is_initialized : concerns_using_a_fake_container
        {
            public override void Context() { }
            public override void Because() { }

            [Observation]
            public void should_be_an_instance_of_IContainer()
            {
                the_kernel.should_be_an_instance_of<IKernel>();
            }
        }

        [Concern(typeof(NinjectContainer))]
        public class when_asking_the_kernel_using_structuremap_for_an_item_and_it_has_that_that_item_registered :
            concerns_using_a_real_container
        {
            private static LogFactory result;

            public override void Context() { the_kernel.Bind<LogFactory>().To<Log4NetLogFactory>(); }
            public override void Because() { result = sut.Resolve<LogFactory>(); }


            [Observation]
            public void should_return_the_item_successfully()
            {
                result.should_be_an_instance_of<Log4NetLogFactory>();
            }
        }

        [Concern(typeof(NinjectContainer))]
        public class when_asking_the_kernel_using_structuremap_to_resolve_an_item_and_it_does_not_have_the_item_registered :
                concerns_using_a_real_container
        {
            private static Action attempting_to_get_an_unregistered_item;

            public override void Context()
            {
                the_kernel.Unbind<LogFactory>();
            }


            public override void Because() { attempting_to_get_an_unregistered_item = () => the_kernel.Get<LogFactory>(); }

            [Observation]
            public void should_throw_an_exception()
            {
                attempting_to_get_an_unregistered_item.should_throw_an<ActivationException>();
            }
        }
    }
}