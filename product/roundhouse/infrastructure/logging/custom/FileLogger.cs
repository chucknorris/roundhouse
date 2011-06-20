namespace roundhouse.infrastructure.logging.custom
{
    using System;
    using System.IO;
    using filesystem;

    public class FileLogger : Logger
    {
        private readonly string log_file_path;
        private readonly FileSystemAccess file_system;

        public FileLogger(string log_file_path, FileSystemAccess file_system)
        {
            this.log_file_path = log_file_path;
            this.file_system = file_system;
        }

        private void log_message(string message)
        {
            string dateTime = string.Format("{0:MM/dd/yyyy HH:mm:ss;ffff} ", DateTime.Now);
            file_system.verify_or_create_directory(file_system.get_directory_name_from(log_file_path));

            File.AppendAllText(log_file_path, dateTime + message, System.Text.Encoding.UTF8);
        }

        public void log_a_debug_event_containing(string message, params object[] args)
        {
            log_message("[DEBUG]: " + string.Format(message, args));
        }

        public void log_an_info_event_containing(string message, params object[] args)
        {
            log_message("[INFO]: " + string.Format(message, args));
        }

        public void log_a_warning_event_containing(string message, params object[] args)
        {
            log_message("[WARN]: " + string.Format(message, args));
        }

        public void log_an_error_event_containing(string message, params object[] args)
        {
            log_message("[ERROR]: " + string.Format(message, args));
        }

        public void log_a_fatal_event_containing(string message, params object[] args)
        {
            log_message("[FATAL]: " + string.Format(message, args));
        }

        public object underlying_type
        {
            get { return file_system; }
        }
    }

    public class ConsoleLogger : Logger
    {
        private void log_message(string message)
        {
            System.Console.WriteLine(message);
        }

        public void log_a_debug_event_containing(string message, params object[] args)
        {
            log_message("[DEBUG]: " + string.Format(message, args));
        }

        public void log_an_info_event_containing(string message, params object[] args)
        {
            log_message("[INFO]: " + string.Format(message, args));
        }

        public void log_a_warning_event_containing(string message, params object[] args)
        {
            log_message("[WARN]: " + string.Format(message, args));
        }

        public void log_an_error_event_containing(string message, params object[] args)
        {
            log_message("[ERROR]: " + string.Format(message, args));
        }

        public void log_a_fatal_event_containing(string message, params object[] args)
        {
            log_message("[FATAL]: " + string.Format(message, args));
        }

        public object underlying_type
        {
            get { return new object(); }
        }
    }
}