namespace roundhouse.infrastructure.logging.custom
{
    using System;
    using extensions;
    using log4net;

    public sealed class Log4NetLogFactory : LogFactory
    {
        public Logger create_logger_bound_to(Object type)
        {
            return new Log4NetLogger(LogManager.GetLogger(type.to_string()));
        }
    }
}