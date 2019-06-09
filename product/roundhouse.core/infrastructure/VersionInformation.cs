using System.Diagnostics;
using System.Reflection;

namespace roundhouse.infrastructure
{
    public class VersionInformation
    {
        public static string get_current_assembly_version()
        {
            string version = string.Empty;
            Assembly executing_assembly = Assembly.GetCallingAssembly();
            string location = executing_assembly != null ? executing_assembly.Location : string.Empty;

            if (!string.IsNullOrEmpty(location))
            {
                version = FileVersionInfo.GetVersionInfo(location).FileVersion;
            }

            return version;
        }
    }
}