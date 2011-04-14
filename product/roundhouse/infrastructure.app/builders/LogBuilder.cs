namespace roundhouse.infrastructure.app.builders
{
    using System.Collections.Generic;
    using filesystem;
    using infrastructure.logging;
    using infrastructure.logging.custom;

    public static class LogBuilder
    {
        public static Logger build(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
        {
            IList<Logger> loggers = new List<Logger>();

            if (configuration_property_holder.MSBuildTask != null)
            {
                Logger msbuild_logger = new MSBuildLogger(configuration_property_holder);
                loggers.Add(msbuild_logger);
            }

            Logger log4net_logger = new Log4NetLogger(configuration_property_holder.Log4NetLogger);
            loggers.Add(log4net_logger);
            //Logger file_logger = new FileLogger(
            //            combine_items_into_one_path(
            //                file_system,
            //                known_folders.change_drop.folder_full_path,
            //                string.Format("{0}_{1}.log", ApplicationParameters.name, known_folders.change_drop.folder_name)
            //                ),
            //            file_system
            //        );
            //loggers.Add(file_logger);

            return new MultipleLogger(loggers);
        }
    }
}