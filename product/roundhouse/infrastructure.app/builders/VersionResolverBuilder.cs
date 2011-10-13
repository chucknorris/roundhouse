namespace roundhouse.infrastructure.app.builders
{
    using System.Collections.Generic;
    using filesystem;
    using resolvers;

    public class VersionResolverBuilder
    {
        public static VersionResolver build(FileSystemAccess file_system, ConfigurationPropertyHolder configuration_property_holder)
        {
            VersionResolver xml_version_finder = new XmlFileVersionResolver(file_system,
                                                                            configuration_property_holder.VersionXPath,
                                                                            configuration_property_holder.VersionFile);
            VersionResolver dll_version_finder = new DllFileVersionResolver(file_system,
                                                                            configuration_property_holder.VersionFile);

            VersionResolver script_number_version_finder = new ScriptfileVersionResolver(file_system,
                                                                                         configuration_property_holder.
                                                                                             UseLastUpScriptAsVersion,
                                                                                            configuration_property_holder.SqlFilesDirectory, configuration_property_holder.UpFolderName);
            IEnumerable<VersionResolver> resolvers;
            if(configuration_property_holder.UseLastUpScriptAsVersion)
            {
                resolvers = new List<VersionResolver> {script_number_version_finder};
            }
            else
            {
                resolvers = new List<VersionResolver> {xml_version_finder, dll_version_finder};      
            }
             

            return new ComplexVersionResolver(resolvers);
        }
    }
}