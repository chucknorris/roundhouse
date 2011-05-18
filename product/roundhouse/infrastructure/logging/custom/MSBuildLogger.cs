using System;

namespace roundhouse.infrastructure.logging.custom
{
    using app;
    using Microsoft.Build.Framework;

    public sealed class MSBuildLogger : Logger
    {
        private readonly IBuildEngine build_engine;
        private readonly object calling_task;

        public MSBuildLogger(ConfigurationPropertyHolder configuration)
        {
            var task = configuration as ITask;
            if (task != null)
            {
                build_engine = task.BuildEngine;
            }

            calling_task = configuration;
        }

        public void log_a_debug_event_containing(string message, params object[] args)
        {
            if (build_engine == null) return;

            build_engine.LogMessageEvent(new BuildMessageEventArgs(
                string.Format(message, args),
                string.Empty,
                calling_task.GetType().Name,
                MessageImportance.Low));
        }

        public void log_an_info_event_containing(string message, params object[] args)
        {
            if (build_engine == null) return;

            build_engine.LogMessageEvent(new BuildMessageEventArgs(
               string.Format(message, args),
               string.Empty,
               calling_task.GetType().Name,
               MessageImportance.Normal));
        }

        public void log_a_warning_event_containing(string message, params object[] args)
        {
            //build_engine.LogMessageEvent(new BuildMessageEventArgs(
            //   string.Format(message, args),
            //   string.Empty,
            //   calling_task.GetType().Name,
            //   MessageImportance.High));
            if (build_engine == null) return;

            build_engine.LogWarningEvent(new BuildWarningEventArgs(
               string.Empty,
               string.Empty,
               string.Empty, 0, 0, 0, 0,
               string.Format(message, args),
               string.Empty, calling_task.GetType().Name));
        }

        public void log_an_error_event_containing(string message, params object[] args)
        {
            if (build_engine == null) return;

            build_engine.LogErrorEvent(new BuildErrorEventArgs(
                string.Empty,
                string.Empty,
                string.Empty, 0, 0, 0, 0,
                string.Format(message, args),
                string.Empty, calling_task.GetType().Name));
        }

        public void log_a_fatal_event_containing(string message, params object[] args)
        {
            log_an_error_event_containing(message, args);
        }

        public object underlying_type
        {
            get { return build_engine; }
        }
    }
}