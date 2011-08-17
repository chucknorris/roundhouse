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
        private static readonly ILog the_logger = LogManager.GetLogger(typeof(Log4NetAppender));
        private static bool used_merged = true;

        public static void configure()
        {
            string assembly_name = ApplicationParameters.log4net_configuration_assembly;
            Stream xml_config_stream;

            try
            {
                xml_config_stream = Assembly.Load(ApplicationParameters.get_merged_assembly_name()).GetManifestResourceStream(ApplicationParameters.log4net_configuration_resource);

                if (xml_config_stream == null)
                {
                    throw new NullReferenceException("Failed to load xml configuration for log4net, consider that assemblies was not merged");
                }
            }
            catch (Exception)
            {
                used_merged = false;
                xml_config_stream = Assembly.Load(assembly_name).GetManifestResourceStream(ApplicationParameters.log4net_configuration_resource);
            }

            XmlConfigurator.Configure(xml_config_stream);

            the_logger.DebugFormat("Configured {0} from assembly {1}", ApplicationParameters.log4net_configuration_resource, used_merged ? ApplicationParameters.get_merged_assembly_name() : assembly_name);
        }

        public static void configure_without_console()
        {
            string assembly_name = ApplicationParameters.log4net_configuration_assembly;
            Stream xml_config_stream;

            try
            {
                xml_config_stream = Assembly.Load(ApplicationParameters.get_merged_assembly_name()).GetManifestResourceStream(ApplicationParameters.log4net_configuration_resource_no_console);

            }
            catch (Exception)
            {
                used_merged = false;
                xml_config_stream = Assembly.Load(assembly_name).GetManifestResourceStream(ApplicationParameters.log4net_configuration_resource_no_console);
            }

            XmlConfigurator.Configure(xml_config_stream);

            the_logger.DebugFormat("Configured {0} from assembly {1}", ApplicationParameters.log4net_configuration_resource_no_console, used_merged ? ApplicationParameters.get_merged_assembly_name() : assembly_name);
        }

        private static bool already_configured_file_appender = false;

        public static void set_file_appender(string output_directory)
        {
            if (!already_configured_file_appender)
            {
                already_configured_file_appender = true;
                var log = LogManager.GetLogger("roundhouse");
                var l = (log4net.Repository.Hierarchy.Logger)log.Logger;

                var layout = new PatternLayout
                {
                    ConversionPattern = "%date [%-5level] - %message%newline"
                };
                layout.ActivateOptions();

                var app = new RollingFileAppender
                {
                    Name = "roundhouse.changes.log.appender",
                    File = Path.Combine(Path.GetFullPath(output_directory), "roundhouse.changes.log"),
                    Layout = layout,
                    AppendToFile = false
                };
                app.ActivateOptions();

                l.AddAppender(app);
            }
        }

    }
}