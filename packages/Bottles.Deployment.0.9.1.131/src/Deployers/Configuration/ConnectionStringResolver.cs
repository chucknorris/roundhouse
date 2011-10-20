using System.Xml;
using FubuCore;

namespace Bottles.Deployment.Deployers.Configuration
{
    public class ConnectionStringResolver : IConnectionStringResolver
    {
        private const string ATTRIBUTE_NAME = "connectionString";
        private readonly DeploymentSettings _settings;

        public ConnectionStringResolver(DeploymentSettings settings)
        {
            _settings = settings;
        }

        public void Resolve(string filename)
        {
            var document = new XmlDocument();
            document.Load(filename);

            foreach (XmlElement element in document.SelectNodes("//add"))
            {
                var connectionString = element.GetAttribute(ATTRIBUTE_NAME);
                connectionString = TemplateParser.Parse(connectionString, _settings.SubstitutionValues());
                element.SetAttribute(ATTRIBUTE_NAME, connectionString);
            }

            document.Save(filename);
        }
    }
}