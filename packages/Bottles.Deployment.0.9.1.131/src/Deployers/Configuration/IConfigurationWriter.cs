using Bottles.Deployment.Configuration;

namespace Bottles.Deployment.Deployers.Configuration
{
    public interface IConfigurationWriter
    {
        void Write(string filename, ProfileBase profile);
    }
}