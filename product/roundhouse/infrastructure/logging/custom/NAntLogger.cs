namespace roundhouse.infrastructure.logging.custom
{
    using System;
    using app;
    using NAnt.Core;

    public sealed class NAntLogger : Logger
    {
        private readonly Task nant_task;

        public NAntLogger(ConfigurationPropertyHolder configuration)
        {
            nant_task = configuration.NAntTask;
        }

        private void log_message(Level log_level, string message)
        {
            if (nant_task == null) return;
           
            try
            {
                nant_task.Project.Log(log_level, message);
            }
            catch (Exception)
            {
                //move on
            }
        }

        public void log_a_debug_event_containing(string message, params object[] args)
        {
            log_message(Level.Debug, string.Format(message, args));
        }

        public void log_an_info_event_containing(string message, params object[] args)
        {
            log_message(Level.Info, string.Format(message, args));
        }

        public void log_a_warning_event_containing(string message, params object[] args)
        {
            log_message(Level.Warning, string.Format(message, args));
        }

        public void log_an_error_event_containing(string message, params object[] args)
        {
            log_message(Level.Error, string.Format(message, args));
        }

        public void log_a_fatal_event_containing(string message, params object[] args)
        {
            log_message(Level.Error, string.Format(message, args));
        }
    }
}