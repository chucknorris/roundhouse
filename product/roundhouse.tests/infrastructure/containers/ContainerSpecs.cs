using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace roundhouse.tests.infrastructure.containers
{
    using System;
    using roundhouse.infrastructure.containers;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;
    using Container = roundhouse.infrastructure.containers.Container;

    public class ContainerSpecs
    {
        public abstract class concern_for_container : IDisposable
        {
            protected LogFactory result;
            protected InversionContainer the_container;

            protected concern_for_container()
            {
                the_container = Substitute.For<InversionContainer>();
            }

            public void Dispose()
            {
                //Container.initialize_with(null);
            }
        }

        public class when_asking_the_container_to_initialize : concern_for_container
        {

            public when_asking_the_container_to_initialize()
            {
            }
            
            // TODO: What are we really testing here?
            [Fact]
            public void should_not_be_of_type_IWindsorContainer()
            {
                the_container.Should().BeAssignableTo<InversionContainer>();
            }
        }

        public class when_asking_the_container_to_resolve_an_item_and_it_has_the_item_registered : concern_for_container
        {
            public when_asking_the_container_to_resolve_an_item_and_it_has_the_item_registered()
            {
                the_container.Resolve<LogFactory>().Returns(new Log4NetLogFactory());
                Container.initialize_with(the_container);
                result = Container.get_an_instance_of<LogFactory>();
            }

            [Fact]
            public void should_be_an_instance_of_Log4NetLogFactory()
            {
                result.Should().BeOfType<Log4NetLogFactory>();
            }

            //TODO: What does this test exactly test? That Log4NetLogFactory is a subclass of LogFactory?
            [Fact]
            public void should_be_an_instance_of_LogFactory()
            {
                result.Should().BeAssignableTo<LogFactory>();
            }
        }

        public class when_asking_the_container_to_resolve_an_item_and_it_does_not_have_the_item_registered : ContainerSpecs.concern_for_container
        {

            public when_asking_the_container_to_resolve_an_item_and_it_does_not_have_the_item_registered()
            {
                the_container.Resolve<LogFactory>().Throws(
                    new Exception(string.Format("Had an error finding components registered for {0}.",
                        typeof(LogFactory))));

                Container.initialize_with(the_container);
                attempting_to_get_an_unregistered_item = () => the_container.Resolve<LogFactory>();
            }

            readonly Action attempting_to_get_an_unregistered_item;

            [Fact]
            public void ShouldThrow_exception()
            {
                attempting_to_get_an_unregistered_item.ShouldThrow<Exception>();
            }
        }
    }
}