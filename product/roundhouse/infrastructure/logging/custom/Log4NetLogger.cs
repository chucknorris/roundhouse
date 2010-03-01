namespace roundhouse.infrastructure.logging.custom
{
    using log4net;

    public sealed class Log4NetLogger : SubLogger
    {
        private readonly log4net.ILog _logger;

        public Log4NetLogger(ILog logger)
        {
            _logger = logger;
        }
        public Log4NetLogger(ConfigurationPropertyHolder configuration)
        {
            this._logger = configuration.Log4NetLogger;
            //_logger.DebugFormat("Initializing {0}<{1}>", GetType().FullName, logger.Logger.Name);
        }

        public void log_a_debug_event_containing(string message, params object[] args)
        {
            _logger.DebugFormat(message, args);
        }

        public void log_an_info_event_containing(string message, params object[] args)
        {
            _logger.InfoFormat(message, args);
        }

        public void log_a_warning_event_containing(string message, params object[] args)
        {
            _logger.WarnFormat(message, args);
        }

        public void log_an_error_event_containing(string message, params object[] args)
        {
            _logger.ErrorFormat(message, args);
        }

        public void log_a_fatal_event_containing(string message, params object[] args)
        {
            _logger.FatalFormat(message, args);
        }
    }
}