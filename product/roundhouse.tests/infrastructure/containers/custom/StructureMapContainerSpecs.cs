using Moq;

namespace roundhouse.tests.infrastructure.containers.custom
{
    using System;
    using roundhouse.infrastructure.containers.custom;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;
    using StructureMap;
    using Container = roundhouse.infrastructure.containers.Container;

    public class StructureMapContainerSpecs
    {
        public abstract class concern_for_structuremap_container : TinySpec<StructureMapContainer>
        {
            [CLSCompliant(false)]
            protected IContainer the_container;
            protected Mock<IContainer> container_mock = new Mock<IContainer>();


            public override void AfterEachSpec() => Container.initialize_with(null);
        }

        public abstract class concerns_using_a_fake_container : concern_for_structuremap_container
        {
            protected override StructureMapContainer sut { get; }

            protected concerns_using_a_fake_container()
            {
                the_container = container_mock.Object;
                sut = new StructureMapContainer(the_container);
            }
        }

        public abstract class concerns_using_a_real_container : concern_for_structuremap_container
        {
            protected override StructureMapContainer sut { get; }
            public concerns_using_a_real_container()
            {
                the_container = ObjectFactory.Container;
                sut = new StructureMapContainer(the_container);
            }
        }

        [Concern(typeof(StructureMapContainer))]
        public class when_the_container_is_initialized : concerns_using_a_fake_container
        {
            public override void Context() { }
            public override void Because() { }

            [Observation]
            public void should_be_an_instance_of_IContainer()
            {
                the_container.should_be_an_instance_of<IContainer>();
            }
        }

        [Concern(typeof(StructureMapContainer))]
        public class when_asking_the_container_for_an_item_and_it_has_that_that_item_registered :
            concerns_using_a_fake_container
        {
            private LogFactory result;

            public override void Context()
            {
                container_mock.Setup(x => x.GetInstance<LogFactory>()).Returns(new Log4NetLogFactory());
            }

            public override void Because() { result = sut.Resolve<LogFactory>(); }

            [Observation]
            public void should_return_the_item_successfully()
            {
                result.should_be_an_instance_of<LogFactory>();
            }
        }

        [Concern(typeof(StructureMapContainer))]
        public class when_asking_the_container_using_structuremap_for_an_item_and_it_has_that_that_item_registered :
            concerns_using_a_real_container
        {
            private static LogFactory result;

            public override void Context() { the_container.Configure(c => c.For<LogFactory>().Use<Log4NetLogFactory>()); }
            public override void Because() { result = sut.Resolve<LogFactory>(); }


            [Observation]
            public void should_return_the_item_successfully()
            {
                result.should_be_an_instance_of<Log4NetLogFactory>();
            }
        }

        [Concern(typeof(StructureMapContainer))]
        public class when_asking_the_container_to_resolve_an_item_and_it_does_not_have_the_item_registered :
            concerns_using_a_fake_container
        {
            private static Action attempting_to_get_an_unregistered_item;

            public override void Context()
            {
                container_mock.Setup(x => x.GetInstance<LogFactory>()).Throws(
                    new Exception(typeof(LogFactory).AssemblyQualifiedName));
            }

            public override void Because()
            {
                attempting_to_get_an_unregistered_item = () => the_container.GetInstance<LogFactory>();
            }

            [Observation]
            public void should_throw_an_exception()
            {
                attempting_to_get_an_unregistered_item.should_throw_an<Exception>();
            }
        }

        [Concern(typeof(StructureMapContainer))]
        public class when_asking_the_container_using_structuremap_to_resolve_an_item_and_it_does_not_have_the_item_registered :
                concerns_using_a_real_container
        {
            private static Action attempting_to_get_an_unregistered_item;

            public override void Context()
            {
                the_container.EjectAllInstancesOf<LogFactory>();
            }


            public override void Because() { attempting_to_get_an_unregistered_item = () => the_container.GetInstance<LogFactory>(); }

            [Observation]
            public void should_throw_an_exception()
            {
                attempting_to_get_an_unregistered_item.should_throw_an<StructureMapException>();
            }
        }
    }
}