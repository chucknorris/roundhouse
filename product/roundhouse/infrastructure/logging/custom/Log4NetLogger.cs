namespace roundhouse.infrastructure.logging.custom
{
    public sealed class Log4NetLogger : Logger
    {
        private readonly log4net.ILog logger;

        public Log4NetLogger(log4net.ILog logger)
        {
            this.logger = logger;
            //logger.DebugFormat("Initializing {0}<{1}>", GetType().FullName, logger.Logger.Name);
        }

        public void log_a_debug_event_containing(string message, params object[] args)
        {
            logger.DebugFormat(message, args);
        }

        public void log_an_info_event_containing(string message, params object[] args)
        {
            logger.InfoFormat(message, args);
        }

        public void log_a_warning_event_containing(string message, params object[] args)
        {
            logger.WarnFormat(message, args);
        }

        public void log_an_error_event_containing(string message, params object[] args)
        {
            logger.ErrorFormat(message, args);
        }

        public void log_a_fatal_event_containing(string message, params object[] args)
        {
            logger.FatalFormat(message, args);
        }
    }
}