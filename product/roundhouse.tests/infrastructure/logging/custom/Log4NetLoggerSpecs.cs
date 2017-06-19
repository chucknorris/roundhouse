using log4net;
using log4net.Core;
using Moq;

namespace roundhouse.tests.infrastructure.logging.custom
{
    using roundhouse.infrastructure.logging;
    using roundhouse.infrastructure.logging.custom;

    public class Log4NetLoggerSpecs
    {
        public abstract class concern_for_Log4NetLogger : TinySpec<Log4NetLogger>
        {
            protected log4net.ILog log4net_logger;
            protected Mock<ILog> mock_logger = new Mock<log4net.ILog>();
            protected override Log4NetLogger sut => new Log4NetLogger(log4net_logger);

            public override void Context()
            {
                log4net_logger = mock_logger.Object;
            }
        }

        [Concern(typeof(Log4NetLogger))]
        public class when_creating_the_log4netlogger : concern_for_Log4NetLogger
        {
            protected object result;

            public override void Because() { }

            //because b = () => sut.log_an_info_event_containing("dude");

            [Observation]
            public void should_be_an_instance_of_Logger()
            {
                sut.should_be_an_instance_of<Logger>();
            }
        }


        [Concern(typeof(Log4NetLogger))]
        public class when_calling_debug_on_the_logger : concern_for_Log4NetLogger
        {
            public override void Context()
            {
                mock_logger.Setup(x => x.DebugFormat("dude"));
                log4net_logger = mock_logger.Object;
            }

            public override void Because() => sut.log_a_debug_event_containing("dude");

            [Observation]
            public void should_have_called_debug_format_on_the_internal_logger()
            {
                mock_logger.Verify(x => x.DebugFormat("dude"));
            }
        }

        [Concern(typeof(Log4NetLogger))]
        public class when_calling_info_on_the_logger : concern_for_Log4NetLogger
        {

            public override void Context()
            {
                mock_logger.Setup(x => x.InfoFormat("dude"));
                log4net_logger = mock_logger.Object;
            }

            public override void Because() => sut.log_an_info_event_containing("dude");

            [Observation]
            public void should_have_called_info_format_on_the_internal_logger()
            {
                mock_logger.Verify(x => x.InfoFormat("dude"));
            }
        }

        [Concern(typeof(Log4NetLogger))]
        public class when_calling_warn_on_the_logger : concern_for_Log4NetLogger
        {

            public override void Context()
                                    {
                                        mock_logger.Setup(x => x.WarnFormat("dude"));
                                        log4net_logger = mock_logger.Object;
                                    }

            public override void Because() => sut.log_a_warning_event_containing("dude");

            [Observation]
            public void should_have_called_warn_format_on_the_internal_logger()
            {
                mock_logger.Verify(x => x.WarnFormat("dude"));
            }
        }

        [Concern(typeof(Log4NetLogger))]
        public class when_calling_error_on_the_logger : concern_for_Log4NetLogger
        {
            public override void Context()
                                    {
                                        mock_logger.Setup(x => x.ErrorFormat("dude"));
                                        log4net_logger = mock_logger.Object;
                                    }

            public override void Because() => sut.log_an_error_event_containing("dude");

            [Observation]
            public void should_have_called_error_format_on_the_internal_logger()
            {
                mock_logger.Verify(x => x.ErrorFormat("dude"));
            }
        }

        [Concern(typeof(Log4NetLogger))]
        public class when_calling_fatal_on_the_logger : concern_for_Log4NetLogger
        {

            public override void Context()
                                    {
                                        mock_logger.Setup(x => x.FatalFormat("dude"));
                                        log4net_logger = mock_logger.Object;
                                    }

            public override void Because() => sut.log_a_fatal_event_containing("dude");

            [Observation]
            public void should_have_called_fatal_format_on_the_internal_logger()
            {
                mock_logger.Verify(x => x.FatalFormat("dude"));
            }
        }
    }
}