using Moq;

namespace roundhouse.tests.infrastructure.containers
{
    using System;
    using roundhouse.infrastructure.containers;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;
    using Container = roundhouse.infrastructure.containers.Container;

    public class ContainerSpecs
    {
        public abstract class concern_for_container : TinySpec
        {
            protected LogFactory result;
            protected InversionContainer the_container;

            protected Mock<InversionContainer> container_mock;

            public concern_for_container()
            {
                container_mock = new Mock<InversionContainer>();
                the_container = container_mock.Object;
                Container.initialize_with(the_container);
            }

            public override void AfterEachSpec()
            {
                Container.initialize_with(null);
            }
        }

        [Concern(typeof(Container))]
        public class when_asking_the_container_to_resolve_an_item_and_it_has_the_item_registered : concern_for_container
        {
            public override void Context()
            {
                container_mock.Setup(x => x.Resolve<LogFactory>()).Returns(new Log4NetLogFactory());
            }

            public override void Because() { result = Container.get_an_instance_of<LogFactory>(); }

            [Observation]
            public void should_be_an_instance_of_Log4NetLogFactory()
            {
                result.should_be_an_instance_of<Log4NetLogFactory>();
            }

            [Observation]
            public void should_be_an_instance_of_LogFactory()
            {
                result.should_be_an_instance_of<LogFactory>();
            }
        }

        [Concern(typeof(Container))]
        public class when_asking_the_container_to_resolve_an_item_and_it_does_not_have_the_item_registered : concern_for_container
        {
            static Action attempting_to_get_an_unregistered_item;

            public override void Context()
            {
                container_mock.Setup(x => x.Resolve<LogFactory>()).Throws(
      new Exception(string.Format("Had an error finding components registered for {0}.", typeof(LogFactory))));
            }

            public override void Because() { attempting_to_get_an_unregistered_item = () => the_container.Resolve<LogFactory>(); }

            [Observation]
            public void should_throw_an_exception()
            {
                attempting_to_get_an_unregistered_item.should_throw_an<Exception>();
            }
        }
    }
}