using log4net;
using NSubstitute;
using Xunit;

namespace roundhouse.tests.infrastructure.logging.custom
{
    using roundhouse.infrastructure.logging.custom;

    public class Log4NetLoggerSpecs
    {
        public abstract class concern_for_Log4NetLogger // : observations_for_a_sut_with_a_contract<Logger, Log4NetLogger>
        {
            public Log4NetLogger sut;
            protected ILog the_dependency;
            protected ILog log4net_logger;

            protected concern_for_Log4NetLogger()
            {
                the_dependency = Substitute.For<ILog>();
                log4net_logger = the_dependency;
                sut = new Log4NetLogger(the_dependency);
            }
        }

        public class when_creating_the_log4netlogger : concern_for_Log4NetLogger
        {
            protected static object result;

            //because b = () => sut.log_an_info_event_containing("dude");

            // This test seems meaningless - only tests that MBTest works???
            //[Fact]
            //public void should_be_an_instance_of_Logger()
            //{
            //    sut.Should().BeOfType<Logger>();
            //}
        }


        public class when_calling_debug_on_the_logger : concern_for_Log4NetLogger
        {
            public when_calling_debug_on_the_logger()
            {
                sut.log_a_debug_event_containing("dude");
            }

            [Fact]
            public void should_have_called_debug_format_on_the_internal_logger()
            {
                log4net_logger.Received().DebugFormat("dude");
            }
        }

        public class when_calling_info_on_the_logger : concern_for_Log4NetLogger
        {
            public when_calling_info_on_the_logger()
            {
                sut.log_an_info_event_containing("dude");
            }

            [Fact]
            public void should_have_called_info_format_on_the_internal_logger()
            {
                log4net_logger.Received().InfoFormat("dude");
            }
        }

        public class when_calling_warn_on_the_logger : concern_for_Log4NetLogger
        {
            public when_calling_warn_on_the_logger()
            {
                sut.log_a_warning_event_containing("dude");
            }

            [Fact]
            public void should_have_called_warn_format_on_the_internal_logger()
            {
                log4net_logger.Received().WarnFormat("dude");
            }
        }

        public class when_calling_error_on_the_logger : concern_for_Log4NetLogger
        {
            public when_calling_error_on_the_logger()
            {
                sut.log_an_error_event_containing("dude");
            }

            [Fact]
            public void should_have_called_error_format_on_the_internal_logger()
            {
                log4net_logger.Received().ErrorFormat("dude");
            }
        }

        public class when_calling_fatal_on_the_logger : concern_for_Log4NetLogger
        {
            public when_calling_fatal_on_the_logger()
            {
                sut.log_a_fatal_event_containing("dude");
            }


            [Fact]
            public void should_have_called_fatal_format_on_the_internal_logger()
            {
                log4net_logger.Received().FatalFormat("dude");
            }
        }
    }
}