using System;
using Bottles.Deployment.Configuration;
using FubuCore.Configuration;

namespace Bottles.Deployment.Deployers.Configuration
{
    public class ConfigurationWriter : IConfigurationWriter
    {
        public void Write(string filename, ProfileBase profile)
        {
            var settings = profile.Data;
            XmlSettingsParser.Write(settings, filename);
        }
    }
}