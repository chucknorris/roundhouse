namespace roundhouse.infrastructure.logging
{
    using System;

    public interface LogFactory
    {
        SubLogger create_logger_bound_to(Object type);
    }
}