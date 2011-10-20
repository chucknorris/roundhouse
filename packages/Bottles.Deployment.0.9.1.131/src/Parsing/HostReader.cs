using System;
using System.IO;
using FubuCore;

namespace Bottles.Deployment.Parsing
{
    public static class HostReader
    {
        public static HostManifest ReadFrom(string fileName)
        {
            var parser = new SettingsParser(fileName);
            try
            {
                new FileSystem().ReadTextFile(fileName, parser.ParseText);
            }
            catch (Exception ex)
            {
                var message = "Failed trying to read " + fileName;
                throw new ApplicationException(message, ex);
            }

            var hostName = Path.GetFileNameWithoutExtension(fileName);
            var host = new HostManifest(hostName);


            var settings = parser.Settings;

            host.RegisterSettings(settings);
            host.RegisterBottles(parser.References);

            return host;
        }
    }
}