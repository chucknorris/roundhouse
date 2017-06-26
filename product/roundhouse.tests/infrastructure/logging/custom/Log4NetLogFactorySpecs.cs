using FluentAssertions;
using Xunit;

namespace roundhouse.tests.infrastructure.logging.custom
{
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;

    public class Log4NetLogFactorySpecs
    {
        public abstract class concern_for_Log4NetLogFactory 
        {
            protected LogFactory sut = new Log4NetLogFactory();
        }

        public class when_creating_a_log4netfactory : concern_for_Log4NetLogFactory
        {
            protected object result;

            public when_creating_a_log4netfactory()
            {
                result = sut.create_logger_bound_to(typeof(when_creating_a_log4netfactory));
            }


            [Fact]
            public void should_create_an_object_of_type_Logger()
            {
                result.Should().BeAssignableTo<Logger>();
            }
        }
    }
}