namespace roundhouse.infrastructure.logging.custom
{
    using log4net;

    public sealed class Log4NetLogFactory : LogFactory
    {
        public Logger create_logger_bound_to(object type)
        {
            return new Log4NetLogger(LogManager.GetLogger(type?.GetType()));
        }
    }
}