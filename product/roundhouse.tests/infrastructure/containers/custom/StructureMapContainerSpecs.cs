using FluentAssertions;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace roundhouse.tests.infrastructure.containers.custom
{
    using System;
    using roundhouse.infrastructure.containers;
    using roundhouse.infrastructure.containers.custom;
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;
    using StructureMap;
    using Container = roundhouse.infrastructure.containers.Container;
    using NSubstitute;

    public class StructureMapContainerSpecs
    {
        public abstract class concern_for_structuremap_container : IDisposable
        {
            protected InversionContainer sut;

            protected IContainer the_container;

            public void Dispose()
            {
                Container.initialize_with(null);
            }
        }

        public abstract class concerns_using_a_fake_container : concern_for_structuremap_container
        {
            protected concerns_using_a_fake_container()
            {
                the_container = Substitute.For<IContainer>();
                sut = new StructureMapContainer(the_container);
            }
        }

        public abstract class concerns_using_a_real_container : concern_for_structuremap_container
        {
            protected concerns_using_a_real_container()
            {
                the_container = ObjectFactory.Container;
                sut = new StructureMapContainer(the_container);
            }
        }

        public class when_the_container_is_initialized : concerns_using_a_fake_container
        {
            [Fact]
            public void should_be_an_instance_of_IContainer()
            {
                the_container.Should().BeAssignableTo<IContainer>();
            }
        }

        public class when_asking_the_container_for_an_item_and_it_has_that_that_item_registered :
            concerns_using_a_fake_container
        {
            private LogFactory result;

            public when_asking_the_container_for_an_item_and_it_has_that_that_item_registered()
            {
                the_container.GetInstance<LogFactory>().Returns(new Log4NetLogFactory());
                result = sut.Resolve<LogFactory>();
            }

            [Fact]
            public void should_return_the_item_successfully()
            {
                result.Should().BeAssignableTo<LogFactory>();
            }
        }

        public class when_asking_the_container_using_structuremap_for_an_item_and_it_has_that_that_item_registered :
            concerns_using_a_real_container
        {
            private LogFactory result;

            public when_asking_the_container_using_structuremap_for_an_item_and_it_has_that_that_item_registered()
            {
                the_container.Configure(c => c.For<LogFactory>().Use<Log4NetLogFactory>());
                result = sut.Resolve<LogFactory>();
            }


            [Fact]
            public void should_return_the_item_successfully()
            {
                result.Should().BeOfType<Log4NetLogFactory>();
            }
        }

        public class when_asking_the_container_to_resolve_an_item_and_it_does_not_have_the_item_registered :
            concerns_using_a_fake_container
        {
            private Action attempting_to_get_an_unregistered_item;

            public when_asking_the_container_to_resolve_an_item_and_it_does_not_have_the_item_registered()
            {
                the_container.GetInstance<LogFactory>().Throws(
                    new Exception(typeof(LogFactory).AssemblyQualifiedName));
                attempting_to_get_an_unregistered_item = () => the_container.GetInstance<LogFactory>();
            }

                    [Fact]
            public void ShouldThrow_exception()
            {
                attempting_to_get_an_unregistered_item.ShouldThrow<Exception>();
            }
        }

        public class when_asking_the_container_using_structuremap_to_resolve_an_item_and_it_does_not_have_the_item_registered :
                concerns_using_a_real_container
        {
            private Action attempting_to_get_an_unregistered_item;

            public when_asking_the_container_using_structuremap_to_resolve_an_item_and_it_does_not_have_the_item_registered()
            {
                the_container.EjectAllInstancesOf<LogFactory>();
                attempting_to_get_an_unregistered_item = () => the_container.GetInstance<LogFactory>();
            }

            [Fact]
            public void ShouldThrow_exception()
            {
                attempting_to_get_an_unregistered_item.ShouldThrow<Exception>();
            }
        }
    }
}