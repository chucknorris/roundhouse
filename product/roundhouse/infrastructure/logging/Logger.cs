namespace roundhouse.infrastructure.logging
{
    public interface Logger
    {
        void log_a_debug_event_containing(string message, params object[] args);
        void log_an_info_event_containing(string message, params object[] args);
        void log_a_warning_event_containing(string message, params object[] args);
        void log_an_error_event_containing(string message, params object[] args);
        void log_a_fatal_event_containing(string message, params object[] args);
        object underlying_type { get;}
    }
}