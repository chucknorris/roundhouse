using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;

namespace roundhouse.infrastructure.app.logging
{
    public class Log4NetAppender
    {
        private static ILog _logger = LogManager.GetLogger(typeof (Log4NetAppender));
      
        private static IAppender set_up_console_appender()
        {
            ConsoleAppender appender = new ConsoleAppender();
            appender.Name = "ConsoleAppender";
            
            PatternLayout pattern_layout = new PatternLayout("%message%newline");
            pattern_layout.ActivateOptions();
            appender.Layout = pattern_layout;
            
            appender.ActivateOptions();

            return appender;
        }

        private static IAppender set_up_rolling_file_appender()
        {
            string file_name = ApplicationParameters.logging_file;

            RollingFileAppender appender = new RollingFileAppender();
            appender.Name = "RollingLogFileAppender";
            appender.File = file_name;
            appender.AppendToFile = false;
            appender.StaticLogFileName = true;

            PatternLayout pattern_layout= new PatternLayout("%date [%-5level] - %message%newline");
            pattern_layout.ActivateOptions();
            appender.Layout = pattern_layout;

            appender.ActivateOptions();

            return appender;
        }

        public static void configure()
        {
            //ILoggerRepository log_repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
            //log_repository.Threshold = Level.Info;
            
            //BasicConfigurator.Configure(log_repository, set_up_console_appender());
            //BasicConfigurator.Configure(log_repository,set_up_rolling_file_appender());

            string assembly_name = ApplicationParameters.log4net_configuration_assembly;
            Stream xml_config_stream;
            
            try
            {
                xml_config_stream = Assembly.Load(assembly_name).GetManifestResourceStream(ApplicationParameters.log4net_configuration_resource);
            }
            catch (Exception)
            {
                assembly_name = "rh";
                xml_config_stream = Assembly.Load(assembly_name).GetManifestResourceStream(ApplicationParameters.log4net_configuration_resource);
            }

            XmlConfigurator.Configure(xml_config_stream);

            _logger.DebugFormat("Configured {0} from assembly {1}", assembly_name, ApplicationParameters.log4net_configuration_resource);
        }

        public static void configure_without_console()
        {
            //ILoggerRepository log_repository = LogManager.GetRepository(Assembly.GetCallingAssembly());
            //log_repository.Threshold = Level.Info;

            //BasicConfigurator.Configure(log_repository, set_up_console_appender());
            //BasicConfigurator.Configure(log_repository,set_up_rolling_file_appender());

            string assembly_name = ApplicationParameters.log4net_configuration_assembly;
            Stream xml_config_stream;

            try
            {
                xml_config_stream = Assembly.Load(assembly_name).GetManifestResourceStream(ApplicationParameters.log4net_configuration_resource_no_console);
            }
            catch (Exception)
            {
                assembly_name = "rh";
                xml_config_stream = Assembly.Load(assembly_name).GetManifestResourceStream(ApplicationParameters.log4net_configuration_resource_no_console);
            }

            XmlConfigurator.Configure(xml_config_stream);

            _logger.DebugFormat("Configured {0} from assembly {1}", ApplicationParameters.log4net_configuration_resource_no_console, assembly_name);
        }

    }
}