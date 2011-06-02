using System;

namespace roundhouse.infrastructure.logging.custom
{
    using System.Collections.Generic;

    public sealed class MultipleLogger : Logger
    {
        private readonly IList<Logger> the_loggers;

        public MultipleLogger(IList<Logger> loggers)
        {
            the_loggers = loggers ?? new List<Logger>();
        }

        public void log_a_debug_event_containing(string message, params object[] args)
        {
            foreach (Logger logger in the_loggers)
            {
                logger.log_a_debug_event_containing(message, args);
            }
        }

        public void log_an_info_event_containing(string message, params object[] args)
        {
            foreach (Logger logger in the_loggers)
            {
                logger.log_an_info_event_containing(message, args);
            }
        }

        public void log_a_warning_event_containing(string message, params object[] args)
        {
            foreach (Logger logger in the_loggers)
            {
                logger.log_a_warning_event_containing(message, args);
            }
        }

        public void log_an_error_event_containing(string message, params object[] args)
        {
            foreach (Logger logger in the_loggers)
            {
                logger.log_an_error_event_containing(message, args);
            }
        }

        public void log_a_fatal_event_containing(string message, params object[] args)
        {
            foreach (Logger logger in the_loggers)
            {
                logger.log_a_fatal_event_containing(message, args);
            }
        }

        public object underlying_type
        {
            get { return the_loggers; }
        }
    }
}