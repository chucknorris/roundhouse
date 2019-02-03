using System;
using System.Xml;
using roundhouse.infrastructure.extensions;
using roundhouse.infrastructure.filesystem;
using roundhouse.infrastructure.logging;

namespace roundhouse.resolvers
{
    public sealed class XmlFileVersionResolver : VersionResolver
    {
        private readonly FileSystemAccess file_system;
        private readonly string x_path;
        private readonly string version_file;
        private const string xml_extension = ".xml";

        public XmlFileVersionResolver(FileSystemAccess file_system, string x_path, string version_file)
        {
            this.file_system = file_system;
            this.x_path = x_path;
            this.version_file = file_system.get_full_path(version_file);
        }

        public bool meets_criteria()
        {
            if (version_file_is_xml(version_file))
            {
                return true;
            }

            return false;
        }

        public string resolve_version()
        {
            Log.bound_to(this).log_an_info_event_containing(" Attempting to resolve version from {0} using {1}.",
                                                                version_file, x_path);
            string version = "0";
            XmlDocument xml = new XmlDocument();
            if (file_system.file_exists(version_file))
            {
                try
                {
                    xml.Load(version_file);
                    XmlNode node = xml.SelectSingleNode(x_path);
                    version = node.InnerText;
                    Log.bound_to(this).log_an_info_event_containing(" Found version {0} from {1}.", version, version_file);
                }
                catch (Exception)
                {
                    Log.bound_to(this).log_an_error_event_containing(
                        "Unable to get version from xml file {0} using xpath {1}", version_file, x_path);
                }
            } else
            {
                Log.bound_to(this).log_a_warning_event_containing(
                        "Unable to get version from xml file {0}. File doesn't exist.", version_file);
            }

            return version;
        }

        private bool version_file_is_xml(string version_file)
        {
            return file_system.get_file_extension_from(version_file).to_lower() == xml_extension;
        }


    }
}