namespace roundhouse.infrastructure.app.builders
{
    using System.Collections.Generic;
    using filesystem;
    using resolvers;

    public class VersionResolverBuilder
    {
        public static VersionResolver build(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
        {
            VersionResolver commandLine_version_finder = new CommandLineVersionResolver(configuration_property_holder.Version);

            if (commandLine_version_finder.meets_criteria())
            {
                return commandLine_version_finder;
            }
            else
            {
                VersionResolver xml_version_finder = new XmlFileVersionResolver(file_system, configuration_property_holder.VersionXPath, configuration_property_holder.VersionFile);
                VersionResolver dll_version_finder = new DllFileVersionResolver(file_system, configuration_property_holder.VersionFile);
                VersionResolver text_version_finder = new TextVersionResolver(file_system, configuration_property_holder.VersionFile);
                VersionResolver script_number_version_finder = new ScriptfileVersionResolver(file_system, configuration_property_holder);
            VersionResolver json_version_finder = new JsonVersionResolver(file_system, configuration_property_holder.VersionFile);

            IEnumerable<VersionResolver> resolvers = new List<VersionResolver> { xml_version_finder, dll_version_finder, text_version_finder, script_number_version_finder, json_version_finder };

                return new ComplexVersionResolver(resolvers);
            }
        }
    }
}