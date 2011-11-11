namespace roundhouse.infrastructure.app.logging
{
    using System;
    using System.IO;
    using loaders;

    public class Log4NetDll
    {
        public static bool is_resolved()
        {
            if (ApplicationParameters.get_merged_assembly_name() == ApplicationParameters.default_merged_assembly_name) return true;

            string log4net_file = "log4net.dll";
            bool log4net_dll_exists = false;

            if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, log4net_file)))
            {
                log4net_dll_exists = true;
            }

            if (!log4net_dll_exists)
            {
                //try to resolve it anywhere
                try
                {
                    DefaultAssemblyLoader.load_assembly(log4net_file);
                    log4net_dll_exists = true;
                }
                catch (Exception)
                {
                    //the file doesn't exist 
                }
            }

            return log4net_dll_exists;
        }

    }
}