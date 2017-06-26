using System;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace roundhouse.tests.infrastructure.logging
{
    using roundhouse.infrastructure.containers;
    using roundhouse.infrastructure.containers.custom;
    using roundhouse.infrastructure.logging;

    public class LogSpecs
    {
        public abstract class concern_for_logging : IDisposable
        {
            protected Logger result;

            protected InversionContainer the_container;
            protected LogFactory mock_log_factory;
            protected Logger mock_logger;

            protected concern_for_logging()
            {
                the_container = Substitute.For<InversionContainer>();
                Container.initialize_with(the_container);
            }

            public void Dispose()
            {
                Container.initialize_with(null);
            }

        }

        public class when_asking_for_a_logger_and_one_has_been_registered : concern_for_logging
        {
            public when_asking_for_a_logger_and_one_has_been_registered()
                                    {
                                        mock_log_factory = Substitute.For<LogFactory>();
                                        mock_logger = Substitute.For<Logger>();
                                        the_container.Resolve<LogFactory>().Returns(mock_log_factory);
                                        mock_log_factory.create_logger_bound_to(typeof(StructureMapContainer)).ReturnsForAnyArgs(mock_logger);

                //when(the_container).is_told_to(x => x.Resolve<LogFactory>())
                //    .Return(mock_log_factory);
                //when_the(mock_log_factory).is_told_to(x => x.create_logger_bound_to(typeof(StructureMapContainer)))
                //    .IgnoreArguments()
                //    .Return(mock_logger);

                                        result = Log.bound_to(typeof(StructureMapContainer)); 
                                    }

            

            [Fact]
            public void should_have_called_the_container_to_resolve_a_registered_logger()
            {
                the_container.Received().Resolve<LogFactory>();
            }

            [Fact]
            public void should_not_be_null()
            {
                result.Should().NotBeNull();
            }

            [Fact]
            public void should_return_a_custom_logger()
            {
                //mock_log_factory.Should().Be(result);
                //result.Should().BeOfType<LogFactory>();
                // result.Should().Be(mock_log_factory);
            }
        }
    }
}